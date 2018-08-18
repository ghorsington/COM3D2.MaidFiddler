import json
import os
from maidfiddler.util.util import BASE_DIR
from maidfiddler.util.config import CONFIG
import random

current_translation = {}

def tr(obj):
    return tr_str(obj.whatsThis())

def tr_str(original):
    if "translation" not in current_translation:
        return original
    parts = original.split(".")
    cur = current_translation["translation"]
    for arg in parts:
        if arg not in cur:
            return get_original(original, parts)
        cur = cur[arg]
    return cur

MINIFY = None
def get_original(s, parts):
    global MINIFY
    if MINIFY is None:
        print("Fetching MINIFY")
        MINIFY = CONFIG.getboolean("Developer", "minify-untranslated-tags", fallback=True)
    return s if not MINIFY else parts[-1]

def load_translation(name):
    global current_translation
    path = os.path.join(BASE_DIR, "translations", name)

    print(f"TL path: {path}")

    if not os.path.isfile(path):
        return
    
    with open(path, "r", encoding="utf-8-sig") as tl_file:
        current_translation = json.load(tl_file)

    if "translation" not in current_translation:
        print("translation invalid")
        current_translation = {}
        return

def get_random_title():
    if "titles" not in current_translation or len(current_translation["titles"]) == 0:
        return None
    return random.choice(current_translation["titles"])

def get_language_name(path):
    if not os.path.isfile(path):
        return None
    
    try:
        with open(path, "r", encoding="utf-8-sig") as tl_file:
            tl = json.load(tl_file)
        return tl["info"]["language"]
    except:
        return None