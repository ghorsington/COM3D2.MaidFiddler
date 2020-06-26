import logging

logger = logging.getLogger("MaidFiddler")

def setup_logger():
    logger.setLevel(logging.DEBUG)

    formatter = logging.Formatter('%(asctime)s [%(levelname)s] : %(message)s')

    conoutCh = logging.StreamHandler()
    conoutCh.setLevel(logging.DEBUG)
    conoutCh.setFormatter(formatter)

    fileCh = logging.FileHandler("mf_log.txt", "w")
    fileCh.setLevel(logging.DEBUG)
    fileCh.setFormatter(formatter)

    logger.addHandler(conoutCh)
    logger.addHandler(fileCh)