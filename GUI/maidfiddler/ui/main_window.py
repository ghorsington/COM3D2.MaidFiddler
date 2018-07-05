import PyQt5.uic as uic
from PyQt5.QtCore import Qt
from PyQt5.QtGui import QPixmap, QIcon
from PyQt5.QtWidgets import QHeaderView, QTableWidgetItem, QLineEdit, QCheckBox, QSpinBox, QWidget, QHBoxLayout, QListWidgetItem

import maidfiddler.util.util as util
from maidfiddler.util.eventpoller import EventPoller
from maidfiddler.ui.tabs import *
from maidfiddler.ui.maids_list import MaidsList

UI_MainWindow = uic.loadUiType(
    open(util.get_resource_path("templates/maid_fiddler.ui")))


class MaidManager:
    def __init__(self):
        self.maid_data = {}

    def clear(self):
        self.maid_data.clear()

    def add_maid(self, maid):
        self.maid_data[maid["guid"]] = maid


class MainWindow(UI_MainWindow[1], UI_MainWindow[0]):
    def __init__(self, core, group, close_func):
        print("Initializing UI")
        super(MainWindow, self).__init__()
        self.setupUi(self)
        self.ui_tabs.setEnabled(False)

        self.maid_list_widgets = {}

        self.close_func = close_func
        self.core = core
        self.event_poller = EventPoller(8890)
        self.event_poller.start(self.core, group)

        self.maid_mgr = MaidManager()
        self.tabs = []

        self.tabs.append(MaidInfoTab(self, self.core, self.maid_mgr))
        self.tabs.append(MaidStatsTab(self, self.core, self.maid_mgr))
        self.tabs.append(FeaturePropensityTab(self, self.core, self.maid_mgr))
        self.tabs.append(YotogiTab(self, self.core, self.maid_mgr))
        self.tabs.append(WorkTab(self, self.core, self.maid_mgr))
        self.tabs.append(PlayerTab(self, self.core, self.maid_mgr))
        self.maids_list = MaidsList(self, self.core, self.maid_mgr)

        self.load_ui()
        self.init_events()

        self.maids_list.reload_maids()
        print("UI initialized!")

    def init_events(self):
        self.maids_list.init_events(self.event_poller)

        for tab in self.tabs:
            tab.init_events(self.event_poller)

    def closeEvent(self, event):
        self.core.DisconnectEventHander()
        self.event_poller.stop()
        self.close_func()

    def close(self):
        self.event_poller.stop()

    def load_ui(self):
        print("Getting game information!")
        self.game_data = self.core.GetGameInfo()
        print("Got game info! Intializing the UI...")

        for tab in self.tabs:
            tab.game_data = self.game_data
