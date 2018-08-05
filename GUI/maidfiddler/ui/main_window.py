import PyQt5.uic as uic
from PyQt5.QtCore import Qt
from PyQt5.QtGui import QPixmap, QIcon
from PyQt5.QtWidgets import QHeaderView, QTableWidgetItem, QLineEdit, QCheckBox, QSpinBox, QWidget, QHBoxLayout, QListWidgetItem, QMenu, QAction

import maidfiddler.util.util as util
from maidfiddler.util.eventpoller import EventPoller
from maidfiddler.ui.tabs import *
from maidfiddler.ui.maids_list import MaidsList

from maidfiddler.util.translation import load_translation, tr, get_random_title

UI_MainWindow = uic.loadUiType(
    open(util.get_resource_path("templates/maid_fiddler.ui")))

BASE_TITLE = "Maid Fiddler for COM"

class MaidManager:
    def __init__(self):
        self.maid_data = {}
        self.selected_maid = None

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
        player_tab = PlayerTab(self, self.core, self.maid_mgr)
        self.tabs.append(player_tab)

        self.maids_list = MaidsList(self, self.core, self.maid_mgr)

        self.load_ui()
        self.init_events()
        self.translate_ui("english")

        self.maids_list.reload_maids()
        player_tab.reload_player_props()
        print("UI initialized!")

    def translate_ui(self, language):
        load_translation(f"{language}.json")

        subtitle = get_random_title()
        title = BASE_TITLE
        if subtitle is not None:
            title += f" -- {subtitle}"
        self.setWindowTitle(title)

        for menu_item in self.findChildren(QMenu):
            menu_item.setTitle(tr(menu_item))

        for menu_action in self.findChildren(QAction):
            if len(menu_action.objectName()) != 0:
                menu_action.setText(tr(menu_action))

        for tab in self.tabs:
            tab.translate_ui()

    def init_events(self):
        self.maids_list.init_events(self.event_poller)

        for tab in self.tabs:
            tab.init_events(self.event_poller)
        # Game-related
        self.actionMax_facility_grade.triggered.connect(lambda: self.core.MaxFacilityGrades())
        self.actionUnlock_all_trohpies.triggered.connect(lambda: self.core.UnlockAllTrophies())
        self.actionUnlock_all_stock_items.triggered.connect(lambda: self.core.UnlockAllStockItems())
        self.actionMaximum_credits.triggered.connect(lambda: self.core.MaxCredits())
        self.actionMaximum_club_grade_and_evaluation.triggered.connect(lambda: self.core.MaxGrade())

        self.actionEnglish.triggered.connect(lambda: self.translate_ui("english"))
        self.actionHorse.triggered.connect(lambda: self.translate_ui("neigh"))

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
