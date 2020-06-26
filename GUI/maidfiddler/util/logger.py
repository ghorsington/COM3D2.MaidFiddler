import logging
from util import BASE_DIR
import os.path

logger = logging.getLogger("MaidFiddler")

def setup_logger():
    logger.setLevel(logging.DEBUG)

    formatter = logging.Formatter('%(asctime)s [%(levelname)s] : %(message)s')

    conoutCh = logging.StreamHandler()
    conoutCh.setLevel(logging.DEBUG)
    conoutCh.setFormatter(formatter)

    fileCh = logging.FileHandler(os.path.join(BASE_DIR, "mf_log.txt"), "w")
    fileCh.setLevel(logging.DEBUG)
    fileCh.setFormatter(formatter)

    logger.addHandler(conoutCh)
    logger.addHandler(fileCh)