import json
import os
from maidfiddler.util.util import BASE_DIR

current_translation = {}
FIRST_TIME = True

def tr(obj, original):
    if not FIRST_TIME:
        original = obj.whatsThis()
    else:
        obj.setWhatsThis(original)
    
    if "translation" not in current_translation:
        return original
    parts = original.split(".")
    cur = current_translation["translation"]
    for arg in parts:
        if arg not in cur:
            return original
        cur = cur[arg]
    return cur

def load_translation(name):
    global current_translation
    path = os.path.join(BASE_DIR, "translations", name)

    print(f"TL path: {path}")

    if not os.path.isfile(path):
        print("translation invalid: No file found")
        return
    
    with open(path, "r", encoding="utf-8") as tl_file:
        current_translation = json.load(tl_file)

    if "translation" not in current_translation:
        print("translation invalid")
        current_translation = {}
        return