import configparser
import os
from maidfiddler.util.util import BASE_DIR

CONFIG = configparser.ConfigParser()
CONFIG.read_dict({
        "Options": {
                "language": "english.json",
                "show_startup_warning": "yes",
                "check_updates_on_startup": "yes"
        },
        "Developer": {
                "minify-untranslated-tags": "yes"
        }
})

CONFIG_PATH = os.path.join(BASE_DIR, "config.ini")

def load_config():
    if not os.path.isfile(CONFIG_PATH):
        return

    with open(CONFIG_PATH, "r", encoding="utf-8") as config_file:
        CONFIG.read_file(config_file)

def save_config():
    with open(CONFIG_PATH, "w", encoding="utf-8") as config_file:
        CONFIG.write(config_file)