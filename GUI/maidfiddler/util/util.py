import sys
import os

BASE_DIR = ""

if getattr(sys, "frozen", False):
    BASE_DIR = os.path.dirname(sys.executable)
else:
    BASE_DIR = os.path.dirname(os.path.abspath(sys.argv[0]))

def get_resource_path(rel_path):
    if getattr(sys, "_MEIPASS", False):
        return os.path.join(getattr(sys, "_MEIPASS"), rel_path)
    return rel_path

def open_bytes(path):
    with open(get_resource_path(path), "rb") as file:
        return file.read()