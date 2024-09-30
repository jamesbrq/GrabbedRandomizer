import ctypes
import psutil
from ctypes import wintypes

# Define necessary constants
MEM_COMMIT = 0x1000


# Define the MEMORY_BASIC_INFORMATION structure
class MEMORY_BASIC_INFORMATION(ctypes.Structure):
    _fields_ = [
        ("BaseAddress", wintypes.LPVOID),
        ("AllocationBase", wintypes.LPVOID),
        ("AllocationProtect", wintypes.DWORD),
        ("RegionSize", ctypes.c_size_t),
        ("State", wintypes.DWORD),
        ("Protect", wintypes.DWORD),
        ("Type", wintypes.DWORD),
    ]


# Define the VirtualQueryEx function
VirtualQueryEx = ctypes.windll.kernel32.VirtualQueryEx
OpenProcess = ctypes.windll.kernel32.OpenProcess
CloseHandle = ctypes.windll.kernel32.CloseHandle


def get_memory_regions(process_id):
    process_handle = OpenProcess(0x0010 | 0x0400, False, process_id)  # PROCESS_QUERY_INFORMATION | PROCESS_VM_READ
    if not process_handle:
        raise Exception("Could not open process!")

    memory_info = MEMORY_BASIC_INFORMATION()
    address = 0
    regions = []

    while VirtualQueryEx(process_handle, ctypes.c_void_p(address), ctypes.byref(memory_info),
                         ctypes.sizeof(memory_info)):
        print(hex(memory_info.BaseAddress) if memory_info.BaseAddress else None)
        if memory_info.State == MEM_COMMIT:  # Only interested in committed regions
            regions.append(
                (memory_info.BaseAddress, memory_info.RegionSize, memory_info.AllocationBase, memory_info.Type))
        address += memory_info.RegionSize

    CloseHandle(process_handle)
    return regions


# Function to find subregions within a given region
def find_subregions(base_address, region_size, process_id):
    subregions = []
    address = base_address
    while address < base_address + region_size:
        subregions.append((address, region_size))
        address += region_size // 2  # For example, move by half of the region size for simplicity
    return subregions


# Get the process ID for xemu.exe
xemu_process = None
for proc in psutil.process_iter(['pid', 'name']):
    if proc.info['name'] == 'xemu.exe':
        xemu_process = proc
        break

if xemu_process is None:
    raise Exception("Process not found!")

# Get memory regions
regions = get_memory_regions(xemu_process.pid)

# Print memory regions and check for subregions
for base_address, region_size, allocation_base, mem_type in regions:
    print(
        f"Base Address: {hex(base_address)} - Size: {hex(region_size)} bytes - Allocation Base: {hex(allocation_base)} - Type: {mem_type}")
