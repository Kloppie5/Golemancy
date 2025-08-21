import json
import networkx as nx
import numpy as np
import re
import struct
import sys
import time
from collections import defaultdict
from typing import List, Dict, Tuple, Optional

from driver import APIDriver

_str_re = re.compile(rb"[ -~]{5,}")

def byte_entropy(b: np.ndarray) -> float:
    if b.size == 0:
        return 0.0
    hist = np.bincount(b, minlength=256).astype(np.float64)
    p = hist / hist.sum()
    nz = p[p > 0]
    return float(-(nz * np.log2(nz)).sum())  # 0..8


def find_ascii_strings(b: bytes, max_per_kb: int = 10) -> List[str]:
    out = []
    for m in _str_re.finditer(b):
        s = m.group().decode("ascii", errors="ignore")
        out.append(s)
        if len(out) >= max_per_kb:
            break
    return out

def looks_like_floats(b: np.ndarray) -> float:
    n = (b.size // 4)
    if n == 0:
        return 0.0
    arr = np.frombuffer(b.tobytes(), dtype="<f4", count=n)
    finite = np.isfinite(arr)
    good = np.logical_and(finite, np.abs(arr) < 1e6)
    ratio = good.mean() if good.size else 0.0
    return float(ratio)

def looks_like_ints(b: np.ndarray) -> float:
    n = (b.size // 4)
    if n == 0:
        return 0.0
    arr = np.frombuffer(b.tobytes(), dtype="<u4", count=n)
    ratio = (arr < 10_000_000).mean()
    return float(ratio)

def guess_pointer_offsets(read_bytes: np.ndarray, regions: List[Dict], ptr_size: int = 8, max_candidates: int = 16) -> List[int]:
    offs = []
    b = read_bytes.tobytes()
    step = ptr_size
    for i in range(0, len(b) - ptr_size + 1, step):
        if ptr_size == 8:
            val = struct.unpack_from("<Q", b, i)[0]
        else:
            val = struct.unpack_from("<I", b, i)[0]
        if is_valid_pointer(val, regions):
            offs.append(i)
            if len(offs) >= max_candidates:
                break
    return offs

def is_valid_pointer(addr: int, regions: List[Dict]) -> bool:
    for r in regions:
        base = int(r["baseAddr"])
        size = int(r["size"])
        if base <= addr < base + size:
            return True
    return False

def read_pointer(api: APIDriver, addr: int, ptr_size: int = 8) -> Optional[int]:
    raw = api.read(addr, ptr_size)
    if len(raw) < ptr_size:
        return None
    return struct.unpack("<Q", raw.tobytes())[0] if ptr_size == 8 else struct.unpack("<I", raw.tobytes())[0]

def follow_pointer_chain(api: APIDriver, start_addr: int, regions: List[Dict], depth: int = 2, ptr_size: int = 8) -> List[Tuple[int, Optional[int]]]:
    chain = []
    addr = start_addr
    visited = set()
    for _ in range(depth):
        if addr in visited:
            break
        visited.add(addr)
        val = read_pointer(api, addr, ptr_size)
        chain.append((addr, val))
        if val is None or not is_valid_pointer(val, regions):
            break
        addr = val
    return chain

def rank_regions(api: APIDriver, regions: List[Dict], read_len: int = 1024) -> List[Tuple[float, Dict]]:
    scored = []
    for reg in regions:
        base = int(reg["baseAddr"])
        size = int(reg["size"])
        
        head = api.read(base, min(read_len, size))
        ent = byte_entropy(head)
        s_float = looks_like_floats(head)
        s_int = looks_like_ints(head)
        strings = find_ascii_strings(head.tobytes(), max_per_kb=4)
        
        score = \
            0.4 * (1.0 - abs(ent - 5.0) / 5.0) + \
            0.25 * s_float + \
            0.2 * s_int + \
            (0.15 if strings else 0.0)
        
        scored.append((float(score), reg, ent, strings))

    scored.sort(key=lambda x: x[0], reverse=True)
    return scored


class GuidedExplorer:
    def __init__(self, api: APIDriver, ptr_size: int = 8, graph_out: str = "mem_graph.json"):
        self.api = api
        self.ptr_size = ptr_size
        self.G = nx.DiGraph()
        self.graph_out = graph_out
        self.regions = []

    def refresh_regions(self):
        self.regions = self.api.list_regions()

    def add_chain_to_graph(self, chain: List[Tuple[int, Optional[int]]]):
        for i, (addr, val) in enumerate(chain):
            if addr not in self.G:
                self.G.add_node(addr, type="ptr", seen=time.time())
            if val is not None:
                if val not in self.G:
                    self.G.add_node(val, type="mem", seen=time.time())
                self.G.add_edge(addr, val)

    def persist_graph(self):
        with open(self.graph_out, "w") as f:
            json.dump(nx.node_link_data(self.G, edges="edges"), f)

    def explore_once(self, top_n: int = 5, read_len: int = 1024, read_around: int = 256, pointer_candidates: int = 8):
        self.refresh_regions()

        ranked = rank_regions(self.api, self.regions, read_len=read_len)

        for score, reg, ent, strings in ranked[:top_n]:
            print(f"reg {reg["baseAddr"]:X}-{reg["baseAddr"]+reg["size"]:X};")
            print(f"  > {score} | {ent}; {strings}")
            base = int(reg["baseAddr"])
            size = int(reg["size"])
            if size <= 0:
                continue

            head = self.api.read(base, min(read_len, size))

            poffs = guess_pointer_offsets(head, self.regions, ptr_size=self.ptr_size, max_candidates=pointer_candidates)

            for off in poffs:
                ptr_addr = base + off
                chain = follow_pointer_chain(self.api, ptr_addr, self.regions, depth=3, ptr_size=self.ptr_size)
                if chain:
                    self.add_chain_to_graph(chain)

        self.persist_graph()

# ======================================== Main ========================================

def main():
    # todo some argparse
    args = {
        "base": "http://localhost:5000",
        "iters": 20,
        "sleep": 0.3,
        "top": 6,
        "read-len": 1024,
        "ptr-size": 4,
        "graph-out": "mem_graph.json"
    }

    api = APIDriver(args["base"])
    explorer = GuidedExplorer(api, ptr_size=args["ptr-size"], graph_out=args["graph-out"])

    for i in range(args["iters"]):
        # try:
        explorer.explore_once(top_n=args["top"], read_len=args["read-len"])
        print(f"[{i+1}/{args["iters"]}] Graph: {explorer.G.number_of_nodes()} nodes, {explorer.G.number_of_edges()} edges -> {args["graph-out"]}")
        # except Exception as e:
        #     print("Iteration error:", e, file=sys.stderr)
        time.sleep(args["sleep"])

    print("Done. Graph saved to", args["graph-out"])

if __name__ == "__main__":
    main()