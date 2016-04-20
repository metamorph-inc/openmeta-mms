import os
import sys
import logging


def setup_logger(log_file, handler_name=None):
    logdir = os.path.join(os.getcwd(), 'log')
    #print logdir
    if not os.path.exists(logdir):
        os.makedirs(logdir)

    if handler_name is None:
        log = logging.getLogger()
    else:
        log = logging.getLogger(handler_name)
    log.setLevel(logging.DEBUG)
    fh = logging.FileHandler(os.path.join(logdir, log_file), "a")
    fh.setLevel(logging.DEBUG)
    formatter = logging.Formatter('%(asctime)s: %(levelname)-8s %(message)s',
                                  datefmt="%Y-%m-%d %H:%M:%S")

    fh.setFormatter(formatter)
    log.addHandler(fh)

    return log


def exitwitherror(cmd):
    logger = logging.getLogger()
    logger.error(cmd)
    with open("_FAILED.txt", "a") as err:
        err.write(cmd)
    sys.exit(-1)


