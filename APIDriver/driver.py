
import requests

BASE = "http://localhost:5000"

def hexdump_command(addr, length):
    return requests.get(f"{BASE}/hexdump?addr={addr:x}&len={length}").json()

def read_command(addr, length):
    return requests.get(f"{BASE}/read?addr={addr:x}&len={length}").json()

def regions_command():
    return requests.get(BASE + "/regions").json()


regions = regions_command()
base = regions[0]["baseAddr"]
size = regions[0]["size"]
print(f"Dumping {base}-{base+size}")
data = hexdump_command(base, size)['data']
#print('\n'.join(data))