import PyQt5.uic as uic
import sys
import os
import threading
import time
from PyQt5.QtCore import pyqtSignal, Qt
from PyQt5.QtGui import QPixmap, QIcon

from PyQt5.QtWidgets import (QWidget,
                             QHBoxLayout,
                             QListWidgetItem,
                             QMenu,
                             QAction,
                             QDialog,
                             QApplication,
                             QMessageBox,
                             QCheckBox,
                             QPushButton)

import maidfiddler.util.util as util
from maidfiddler.ui.tabs import *
from maidfiddler.ui.maids_list import MaidsList

from maidfiddler.ui.connect_dialog import ConnectDialog
from maidfiddler.ui.error_dialog import ErrorDialog
from maidfiddler.ui.about_dialog import AboutDialog

from maidfiddler.util.translation import (load_translation,
                                          tr,
                                          get_random_title,
                                          get_language_name,
                                          tr_str)

from maidfiddler.util.config import CONFIG, save_config

from maidfiddler.ui.resources import APP_ICON
from maidfiddler.util.pipes import PipedEventHandler, PipeRpcCaller

from maidfiddler.ui.dialogs.update_checker import UpdateDialog

from app_info import MIN_SUPPORTED_GAME_VERSION, VERSION

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
    connection_lost = pyqtSignal()
    error_occurred = pyqtSignal(dict)
    tabs = []
    maid_list_widgets = {}
    just_launched = True

    def __init__(self):
        print("Initializing UI")
        super(MainWindow, self).__init__()
        self.setupUi(self)

        self.error_occurred.connect(self.display_error_box)
        sys.excepthook = lambda t, v, tr: self.error_occurred.emit(
            {"t": t, "val": v, "traceback": tr})

        icon = QPixmap()
        icon.loadFromData(APP_ICON)
        self.setWindowIcon(QIcon(icon))

        self.ui_tabs.setEnabled(False)
        self.menuSelected_maid.setEnabled(False)

        self.connection_lost.connect(self.on_connection_close)

        self.core = PipeRpcCaller(self.connection_lost)
        self.event_poller = PipedEventHandler(
            "MaidFildderEventEmitter", self.connection_lost)

        self.about_dialog = AboutDialog()
        self.core_version = "?"

        self.maid_mgr = MaidManager()

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

    def display_error_box(self, data):
        print("Trying to show error dialog")
        dialog = ErrorDialog(data["t"], data["val"], data["traceback"])
        dialog.exec()
        self.close()
        QApplication.instance().exit()

    def check_updates(self, silent):
        updater_dialog = UpdateDialog(silent)
        result = updater_dialog.exec()
        if result == QDialog.Accepted:
            self.close()
            QApplication.instance().exit()
            sys.exit(0)

    def connect(self):
        print("showing connection dialog!")
        connect_dialog = ConnectDialog(self, self.core)

        result = connect_dialog.exec()
        if result != QDialog.Accepted:
            QApplication.instance().exit()
            sys.exit(0)
            return

        game_version = self.core.get_GameVersion()
        if game_version < MIN_SUPPORTED_GAME_VERSION:
            error_dialog = QMessageBox(QMessageBox.Critical, 
                                    "Unsupported game version", 
                                    f"Maid Fiddler {VERSION} only supports game version {MIN_SUPPORTED_GAME_VERSION} or newer.\nThis game's build version is {game_version}.\n\nPlease update the game before using this version of Maid Fiddler.", 
                                    QMessageBox.Ok)
            error_dialog.exec()
            self.core.close()
            QApplication.instance().exit()
            sys.exit(0)
            return

        self.event_poller.start_polling()

        game_data = connect_dialog.game_data
        if game_data is None:
            self.on_connection_close()
            return
        connect_dialog.game_data = None
        
        self.core_version = self.core.get_Version()

        for tab in self.tabs:
            tab.game_data = game_data

        self.maids_list.reload_maids()
        self.player_tab.reload_player_props()

        # Reload translations to translate updated UI
        for tab in self.tabs:
            tab.translate_ui()

        # Finally, display a warning message if there is a need
        self.display_warning()

    def display_warning(self):
        if not CONFIG.getboolean("Options", "show_startup_warning", fallback=True) or not self.just_launched:
            return

        self.just_launched = False
        warning_box = QMessageBox(self)
        warning_box.setIcon(QMessageBox.Warning)

        dont_show_cb = QCheckBox(tr_str("warning_dialog.dont_show_again"))
        dont_show_cb.setCheckable(True)
        dont_show_cb.blockSignals(True)

        confirm_button = QPushButton()
        ok_text = tr_str("warning_dialog.ok")
        confirm_button.setText(ok_text)
        confirm_button.setEnabled(False)

        warning_box.addButton(dont_show_cb, QMessageBox.ResetRole)
        warning_box.addButton(confirm_button, QMessageBox.AcceptRole)
        warning_box.setTextFormat(Qt.RichText)
        warning_box.setText(tr_str("warning_dialog.warning"))
        warning_box.setWindowTitle(tr_str("warning_dialog.title"))

        def button_timer():
            for i in range(5, -1, -1):
                time.sleep(1)
                confirm_button.setText(f"{ok_text} ({i})")
            confirm_button.setText(ok_text)
            confirm_button.setEnabled(True)

        threading._start_new_thread(button_timer, ())
        warning_box.exec()

        if dont_show_cb.checkState() == Qt.Checked:
            CONFIG["Options"]["show_startup_warning"] = "no"
            save_config()

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

    def on_connection_close(self):
        if not self.core.is_connected():
            return
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
        self.maids_list.init_events(self.event_poller)

        for tab in self.tabs:
            tab.init_events(self.event_poller)
        # Game-related
        self.actionMax_facility_grade.triggered.connect(
            lambda: self.core.MaxFacilityGrades())
        self.actionUnlock_all_trohpies.triggered.connect(
            lambda: self.core.UnlockAllTrophies())
        self.actionUnlock_all_stock_items.triggered.connect(
            lambda: self.core.UnlockAllStockItems())
        self.actionMaximum_credits.triggered.connect(
            lambda: self.core.MaxCredits())
        self.actionMaximum_club_grade_and_evaluation.triggered.connect(
            lambda: self.core.MaxGrade())
        self.actiontop_bar_cur_save_all_yotogi_bg_visible.toggled.connect(
            lambda c: self.core.SetAllYotogiStagesVisible(c))
        self.actiontop_bar_cur_save_all_dances_visible.toggled.connect(
            lambda c: self.core.SetEnableAllDances(c))
        self.actiontop_bar_all_maid_unlock_ntr_skills.toggled.connect(
            lambda c: self.core.SetAllNTRSkillsVisible(c))

        self.actionAbout.triggered.connect(self.show_about)
        self.actiontop_bar_misc_check_updates.triggered.connect(lambda: self.check_updates(False))

    def show_about(self):
        self.about_dialog.reload(self.core_version)
        self.about_dialog.open()

    def closeEvent(self, event):
        self.close()
        QApplication.instance().exit()

    def close(self):
        self.core.close()
        if self.event_poller.running:
            self.event_poller.stop_polling()
