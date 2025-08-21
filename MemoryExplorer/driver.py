import base64
import numpy as np
import requests
from typing import List, Dict

class APIDriver:
    def __init__(self, base_url: str):
        self.base = base_url.rstrip("/")
        self._regions_cache = None

    def list_regions(self) -> List[Dict]:
        r = requests.get(self.base + "/regions", timeout=10)
        r.raise_for_status()
        regs = r.json()
        self._regions_cache = regs
        return regs

    def read(self, addr: int, length: int) -> np.ndarray:
        url = f"{self.base}/read?addr={addr:x}&len={length}"
        r = requests.get(url, timeout=10)
        r.raise_for_status()
        js = r.json()
        raw = base64.b64decode(js["data"])
        return np.frombuffer(raw, dtype=np.uint8)

    def hexdump(self, addr: int, length: int = 128) -> str:
        url = f"{self.base}/hexdump?addr={addr:x}&len={length}"
        r = requests.get(url, timeout=10)
        r.raise_for_status()
        return r.text
