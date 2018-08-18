import PyQt5.uic as uic
import zerorpc
import sys
import os
from PyQt5.QtCore import Qt
from PyQt5.QtGui import QPixmap, QIcon
from PyQt5.QtWidgets import QHeaderView, QTableWidgetItem, QLineEdit, QCheckBox, QSpinBox, QWidget, QHBoxLayout, QListWidgetItem, QMenu, QAction, QDialog

import maidfiddler.util.util as util
from maidfiddler.util.eventpoller import EventPoller
from maidfiddler.ui.tabs import *
from maidfiddler.ui.maids_list import MaidsList

from maidfiddler.ui.connect_dialog import ConnectDialog
from maidfiddler.ui.error_dialog import ErrorDialog
from maidfiddler.ui.about_dialog import AboutDialog

from maidfiddler.util.translation import load_translation, tr, get_random_title, get_language_name
from maidfiddler.util.config import CONFIG, save_config

from maidfiddler.ui.resources import APP_ICON


UI_MainWindow = uic.loadUiType(
    open(util.get_resource_path("templates/maid_fiddler.ui")))

BASE_TITLE = "Maid Fiddler COM3D2"

class MaidManager:
    def __init__(self):
        self.maid_data = {}
        self.selected_maid = None

    def clear(self):
        self.maid_data.clear()
        self.selected_maid = None

    def add_maid(self, maid):
        self.maid_data[maid["guid"]] = maid

    def update_guid(self, old, new):
        self.maid_data[new] = self.maid_data[old]
        del self.maid_data[old]

    def remove_maid(self, maid_id):
        del self.maid_data[maid_id]


class MainWindow(UI_MainWindow[1], UI_MainWindow[0]):
    def __init__(self, group, close_func):
        print("Initializing UI")
        super(MainWindow, self).__init__()
        self.setupUi(self)

        sys.excepthook = self.display_error_box

        icon = QPixmap()
        icon.loadFromData(APP_ICON)
        self.setWindowIcon(QIcon(icon))

        self.ui_tabs.setEnabled(False)
        self.menuSelected_maid.setEnabled(False)

        self.core = None
        self.connect_dialog = ConnectDialog(self)
        self.close_func = close_func
        self.event_poller = EventPoller(group)

        self.about_dialog = AboutDialog()
        self.core_version = "?"

        self.maid_list_widgets = {}
        self.maid_mgr = MaidManager()
        self.tabs = []

        self.tabs.append(MaidInfoTab(self))
        self.tabs.append(MaidStatsTab(self))
        self.tabs.append(FeaturePropensityTab(self))
        self.tabs.append(YotogiTab(self))
        self.tabs.append(WorkTab(self))
        self.player_tab = PlayerTab(self)
        self.tabs.append(self.player_tab)

        self.maids_list = MaidsList(self)

        self.init_events()
        self.init_translations()

    def display_error_box(self, t, val, traceback):
        print("Trying to show error dialog")
        dialog = ErrorDialog(t, val, traceback)
        dialog.exec()
        sys.exit(0)

    def connect(self):
        self.connect_dialog.reload()

        result = self.connect_dialog.exec()
        if result != QDialog.Accepted:
            sys.exit(0)
        
        self.core = self.connect_dialog.client
        self.connect_dialog.client = None
        game_data = self.connect_dialog.game_data
        self.connect_dialog.game_data = None

        self.core_version = self.core.get_Version()

        open_port = self.core.GetAvailableTcpPort()
        print(f"Got open TCP port: {open_port}")
        self.event_poller.start(open_port, self.core)

        for tab in self.tabs:
            tab.game_data = game_data

        self.maids_list.reload_maids()
        self.player_tab.reload_player_props()
        
        # Reload translations to translate updated UI
        for tab in self.tabs:
            tab.translate_ui()

    def init_translations(self):
        tl_path = os.path.join(util.BASE_DIR, "translations")
        for tl_file in os.listdir(tl_path):
            lang_name = get_language_name(os.path.join(tl_path, tl_file))
            if lang_name is None:
                continue
            action = self.menutop_bar_misc_language.addAction(lang_name)
            action.triggered.connect(self.tl_action(tl_file))
        language = CONFIG.get("Options", "language", fallback="english.json")
        self.translate_ui(language)

    def tl_action(self, language):
        def handler():
            self.translate_ui(language)
        return handler

    def translate_ui(self, language):
        load_translation(language)
        CONFIG["Options"]["language"] = language
        save_config()

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

    def on_connection_close(self, args=None):
        print("Connection closed!")
        self.close()
        self.maids_list.clear_list()

        for menu_action in self.findChildren(QAction):
            if len(menu_action.objectName()) != 0 and menu_action.isCheckable():
                menu_action.blockSignals(True)
                menu_action.setChecked(False)
                menu_action.blockSignals(False)

        self.connect()

    def init_events(self):
        self.event_poller.on("connection_closed", self.on_connection_close)

        self.maids_list.init_events(self.event_poller)

        for tab in self.tabs:
            tab.init_events(self.event_poller)
        # Game-related
        self.actionMax_facility_grade.triggered.connect(lambda: self.core.MaxFacilityGrades())
        self.actionUnlock_all_trohpies.triggered.connect(lambda: self.core.UnlockAllTrophies())
        self.actionUnlock_all_stock_items.triggered.connect(lambda: self.core.UnlockAllStockItems())
        self.actionMaximum_credits.triggered.connect(lambda: self.core.MaxCredits())
        self.actionMaximum_club_grade_and_evaluation.triggered.connect(lambda: self.core.MaxGrade())
        self.actiontop_bar_cur_save_all_yotogi_bg_visible.toggled.connect(lambda c: self.core.SetAllYotogiStagesVisible(c))

        self.actionAbout.triggered.connect(self.show_about)

    def show_about(self):
        self.about_dialog.reload(self.core_version)
        self.about_dialog.open()

    def closeEvent(self, event):
        self.core.DisconnectEventHander()
        self.close()
        self.close_func()

    def close(self):
        self.event_poller.stop()
        self.core.close()
        self.core = None
