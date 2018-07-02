import sys
import os

APP_RUNNING = True

def get_resource_path(rel_path):
    if getattr(sys, "_MEIPASS", False):
        return os.path.join(getattr(sys, "_MEIPASS"), rel_path)
    return rel_path

def open_bytes(path):
    with open(get_resource_path(path), "rb") as file:
        return file.read()