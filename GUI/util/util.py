import sys
import os

def get_resource_path(rel_path):
    if getattr(sys, "_MEIPASS", False):
        return os.path.join(getattr(sys, "_MEIPASS"), rel_path)
    return rel_path