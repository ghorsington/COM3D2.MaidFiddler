from util.eventpoller import EventPoller
import util.util as util
import PyQt5.uic as uic
from PyQt5.QtCore import Qt, QObject
from PyQt5.QtGui import QPixmap, QIcon
from PyQt5.QtWidgets import QHeaderView, QTableWidgetItem, QLineEdit, QCheckBox, QSpinBox, QWidget, QHBoxLayout, QListWidgetItem

UI_MainWindow = uic.loadUiType(
    open(util.get_resource_path("templates/maid_fiddler.ui")))
NO_THUMBNAIL = util.open_bytes("templates/no_thumbnail.png")

class MainWindow(UI_MainWindow[1], UI_MainWindow[0]):
    def __init__(self, core, group, close_func):
        super(MainWindow, self).__init__()
        self.setupUi(self)

        self.close_func = close_func
        self.core = core
        self.event_poller = EventPoller(8890)
        self.event_poller.start(self.core, group)

        self.maid_info_tab = MaidInfoTab(self, self.event_poller)
        self.maid_stats_tab = MaidStatsTab(self, self.event_poller)
        self.feature_propensity_tab = FeaturePropensityTab(
            self, self.event_poller)
        self.yotogi_tab = YotogiTab(self, self.event_poller)
        self.work_tab = WorkTab(self, self.event_poller)
        self.player_tab = PlayerTab(self, self.event_poller)

        self.load_ui()
        self.init_events()

    def init_events(self):
        self.maid_info_tab.init_events()
        self.maid_stats_tab.init_events()
        self.feature_propensity_tab.init_events()
        self.yotogi_tab.init_events()
        self.work_tab.init_events()
        self.player_tab.init_events()

        self.event_poller.on("maid_added", self.on_maid_added)
        self.event_poller.on(
            "maid_removed", lambda args: print("REMOVE: " + str(args)))
        #self.event_poller.on("maid_prop_changed", lambda args: print(str(args)))

    def on_maid_added(self, args):
        maid_data = args["maid"]
        if "maid_thumbnail" in maid_data and maid_data["maid_thumbnail"] is not None:
            thumb_image = maid_data["maid_thumbnail"]
        else:
            thumb_image = NO_THUMBNAIL

        thumb = QPixmap()
        thumb.loadFromData(thumb_image)

        self.maid_list.addItem(QListWidgetItem(QIcon(thumb), f"{maid_data['set_properties']['firstName']} {maid_data['set_properties']['lastName']}"))

    def closeEvent(self, event):
        self.event_poller.stop()
        self.close_func()

    def close(self):
        self.event_poller.stop()

    def load_ui(self):
        print("Getting game information!")
        self.game_data = self.core.GetGameInfo()
        print("Got game info! Intializing the UI...")

        self.maid_info_tab.game_data = self.game_data
        self.maid_stats_tab.game_data = self.game_data
        self.feature_propensity_tab.game_data = self.game_data
        self.yotogi_tab.game_data = self.game_data
        self.work_tab.game_data = self.game_data
        self.player_tab.game_data = self.game_data

        """ maid_data = self.core.GetMaidData("3eb")
        thumb = QPixmap()
        result = thumb.loadFromData(maid_data["maid_thumbnail"])

        self.maid_list.addItem(QListWidgetItem(QIcon(thumb), "%s %s" % (
            maid_data["set_properties"]["firstName"], maid_data["set_properties"]["lastName"]))) """


class UiTab(QObject):
    def __init__(self, ui, event_poller):
        QObject.__init__(self)
        self.ui = ui
        self.event_poller = event_poller
        self._game_data = None

    @property
    def game_data(self):
        return self._game_data

    @game_data.setter
    def game_data(self, value):
        self._game_data = value
        self.update_ui()

    def init_events(self):
        pass

    def update_ui(self):
        raise NotImplementedError()


class MaidInfoTab(UiTab):
    def update_ui(self):
        for personal in self._game_data["personal_list"]:
            self.ui.personality_combo.addItem(personal["name"], personal["id"])

        for (contract_name, contract_id) in self._game_data["contract"].items():
            self.ui.contract_combo.addItem(contract_name, contract_id)

        for (relation_name, relation_id) in self._game_data["relation"].items():
            self.ui.relation_combo.addItem(relation_name, relation_id)

        for (seik_name, seik_id) in self._game_data["seikeiken"].items():
            self.ui.current_combo.addItem(seik_name, seik_id)
            self.ui.initial_combo.addItem(seik_name, seik_id)

        for job_class in self._game_data["job_class_list"]:
            self.ui.job_class_combo.addItem(job_class["name"], job_class["id"])

        for yotogi_class in self._game_data["yotogi_class_list"]:
            self.ui.yotogi_class_combo.addItem(
                yotogi_class["name"], yotogi_class["id"])

        self.ui.yotogi_class_combo.view().setVerticalScrollBarPolicy(Qt.ScrollBarAsNeeded)


class MaidStatsTab(UiTab):
    def update_ui(self):
        self.ui.maid_params_lockable_table      \
            .horizontalHeader()                 \
            .setSectionResizeMode(0, QHeaderView.Stretch)

        self.ui.maid_params_lockable_table      \
            .horizontalHeader()                 \
            .setSectionResizeMode(1, QHeaderView.ResizeToContents)

        self.ui.maid_params_lockable_table      \
            .horizontalHeader()                 \
            .setSectionResizeMode(2, QHeaderView.ResizeToContents)

        self.ui.maid_params_lockable_table.setRowCount(
            len(self.game_data["maid_status_settable"]))

        for (i, maid_prop) in enumerate(self.game_data["maid_status_settable"]):
            name = QTableWidgetItem(maid_prop)
            line = QLineEdit()
            line.setStyleSheet("width: 15em;")

            checkbox = QCheckBox()
            widget = QWidget()
            hbox = QHBoxLayout(widget)
            hbox.addWidget(checkbox)
            hbox.setAlignment(Qt.AlignCenter)
            hbox.setContentsMargins(0, 0, 0, 0)
            widget.setLayout(hbox)

            self.ui.maid_params_lockable_table.setItem(i, 0, name)
            self.ui.maid_params_lockable_table.setCellWidget(i, 1, line)
            self.ui.maid_params_lockable_table.setCellWidget(i, 2, widget)

        # Maid bonus stats table

        self.ui.maid_params_bonus_table     \
            .horizontalHeader()             \
            .setSectionResizeMode(0, QHeaderView.Stretch)
        self.ui.maid_params_bonus_table     \
            .horizontalHeader()             \
            .setSectionResizeMode(1, QHeaderView.ResizeToContents)

        self.ui.maid_params_bonus_table     \
            .setRowCount(len(self.game_data["maid_bonus_status"]))

        for (i, maid_prop) in enumerate(self.game_data["maid_bonus_status"]):
            name = QTableWidgetItem(maid_prop)
            line = QLineEdit()
            line.setProperty("prop_name", maid_prop)
            line.textChanged.connect(self.handle_line_edit)
            line.setStyleSheet("width: 15em;")

            self.ui.maid_params_bonus_table.setItem(i, 0, name)
            self.ui.maid_params_bonus_table.setCellWidget(i, 1, line)

    def handle_line_edit(self, text):
        obj = QObject.sender(self)
        print("New text: %s from %s" % (text, obj.property("prop_name")))


class FeaturePropensityTab(UiTab):

    def update_ui(self):
        # Feature list

        for feature in self.game_data["feature_list"]:
            item = QListWidgetItem(feature["name"])
            item.setData(Qt.UserRole, feature["id"])
            item.setFlags(item.flags() | Qt.ItemIsUserCheckable)
            item.setCheckState(Qt.Unchecked)
            self.ui.feature_list.addItem(item)

        # Propensity list

        for propensity in self.game_data["propensity_list"]:
            item = QListWidgetItem(propensity["name"])
            item.setData(Qt.UserRole, propensity["id"])
            item.setFlags(item.flags() | Qt.ItemIsUserCheckable)
            item.setCheckState(Qt.Unchecked)
            self.ui.propensity_list.addItem(item)


class YotogiTab(UiTab):
    def update_ui(self):
        self.ui.yotogi_skills_table.setRowCount(
            len(self.game_data["yotogi_skills"]))

        yotogi_skills_header = self.ui.yotogi_skills_table.horizontalHeader()

        yotogi_skills_header.setSectionResizeMode(
            0, QHeaderView.ResizeToContents)
        yotogi_skills_header.setSectionResizeMode(1, QHeaderView.Stretch)
        yotogi_skills_header.setSectionResizeMode(
            2, QHeaderView.ResizeToContents)
        yotogi_skills_header.setSectionResizeMode(
            3, QHeaderView.ResizeToContents)
        yotogi_skills_header.setSectionResizeMode(
            4, QHeaderView.ResizeToContents)

        for (i, skill) in enumerate(self.game_data["yotogi_skills"]):
            name = QTableWidgetItem(skill["name"])
            line_level = QSpinBox()
            line_level.setMinimum(0)
            line_exp = QSpinBox()
            line_exp.setMinimum(0)
            line_play_count = QSpinBox()
            line_play_count.setMinimum(0)

            checkbox = QCheckBox()
            widget = QWidget()
            hbox = QHBoxLayout(widget)
            hbox.addWidget(checkbox)
            hbox.setAlignment(Qt.AlignCenter)
            hbox.setContentsMargins(0, 0, 0, 0)
            widget.setLayout(hbox)

            self.ui.yotogi_skills_table.setCellWidget(i, 0, widget)
            self.ui.yotogi_skills_table.setItem(i, 1, name)
            self.ui.yotogi_skills_table.setCellWidget(i, 2, line_level)
            self.ui.yotogi_skills_table.setCellWidget(i, 3, line_exp)
            self.ui.yotogi_skills_table.setCellWidget(i, 4, line_play_count)


class WorkTab(UiTab):

    def update_ui(self):
        noon_work = [data for data in self.game_data["work_data"]
                     if data["work_type"] != "Yotogi"]

        self.ui.noon_work_table.setRowCount(len(noon_work))

        noon_work_header = self.ui.noon_work_table.horizontalHeader()

        noon_work_header.setSectionResizeMode(0, QHeaderView.ResizeToContents)
        noon_work_header.setSectionResizeMode(1, QHeaderView.Stretch)
        noon_work_header.setSectionResizeMode(2, QHeaderView.ResizeToContents)
        noon_work_header.setSectionResizeMode(3, QHeaderView.ResizeToContents)

        for (i, work_data) in enumerate(noon_work):
            self.ui.cur_noon_work_combo.addItem(
                work_data["name"], work_data["id"])

            name = QTableWidgetItem(work_data["name"])
            line_level = QSpinBox()
            line_level.setMinimum(0)
            line_play_count = QSpinBox()
            line_play_count.setMinimum(0)

            checkbox = QCheckBox()
            widget = QWidget()
            hbox = QHBoxLayout(widget)
            hbox.addWidget(checkbox)
            hbox.setAlignment(Qt.AlignCenter)
            hbox.setContentsMargins(0, 0, 0, 0)
            widget.setLayout(hbox)

            self.ui.noon_work_table.setCellWidget(i, 0, widget)
            self.ui.noon_work_table.setItem(i, 1, name)
            self.ui.noon_work_table.setCellWidget(i, 2, line_level)
            self.ui.noon_work_table.setCellWidget(i, 3, line_play_count)

        # Yotogi work

        yotogi_work = [data for data in self.game_data["work_data"]
                       if data["work_type"] == "Yotogi"]

        self.ui.yotogi_work_table.setRowCount(len(yotogi_work))

        yotogi_work_header = self.ui.yotogi_work_table.horizontalHeader()

        yotogi_work_header.setSectionResizeMode(
            0, QHeaderView.ResizeToContents)
        yotogi_work_header.setSectionResizeMode(1, QHeaderView.Stretch)
        yotogi_work_header.setSectionResizeMode(
            2, QHeaderView.ResizeToContents)
        yotogi_work_header.setSectionResizeMode(
            3, QHeaderView.ResizeToContents)

        for (i, work_data) in enumerate(yotogi_work):
            self.ui.cur_night_work_combo.addItem(
                work_data["name"], work_data["id"])

            name = QTableWidgetItem(work_data["name"])
            line_level = QSpinBox()
            line_level.setMinimum(0)
            line_play_count = QSpinBox()
            line_play_count.setMinimum(0)

            checkbox = QCheckBox()
            widget = QWidget()
            hbox = QHBoxLayout(widget)
            hbox.addWidget(checkbox)
            hbox.setAlignment(Qt.AlignCenter)
            hbox.setContentsMargins(0, 0, 0, 0)
            widget.setLayout(hbox)

            self.ui.yotogi_work_table.setCellWidget(i, 0, widget)
            self.ui.yotogi_work_table.setItem(i, 1, name)
            self.ui.yotogi_work_table.setCellWidget(i, 2, line_level)
            self.ui.yotogi_work_table.setCellWidget(i, 3, line_play_count)


class PlayerTab(UiTab):

    def update_ui(self):
        self.ui.player_params_table.setRowCount(
            len(self.game_data["club_status_settable"]))

        player_params_header = self.ui.player_params_table.horizontalHeader()

        player_params_header.setSectionResizeMode(0, QHeaderView.Stretch)
        player_params_header.setSectionResizeMode(
            2, QHeaderView.ResizeToContents)
        player_params_header.setSectionResizeMode(
            3, QHeaderView.ResizeToContents)

        for (i, param) in enumerate(self.game_data["club_status_settable"]):
            name = QTableWidgetItem(param)
            line = QLineEdit()
            line.setStyleSheet("width: 15em;")

            checkbox = QCheckBox()
            widget = QWidget()
            hbox = QHBoxLayout(widget)
            hbox.addWidget(checkbox)
            hbox.setAlignment(Qt.AlignCenter)
            hbox.setContentsMargins(0, 0, 0, 0)
            widget.setLayout(hbox)

            self.ui.player_params_table.setItem(i, 0, name)
            self.ui.player_params_table.setCellWidget(i, 1, line)
            self.ui.player_params_table.setCellWidget(i, 2, widget)

        # Player flags

        player_flags_header = self.ui.player_flags_table.horizontalHeader()

        player_flags_header.setSectionResizeMode(0, QHeaderView.Stretch)
        player_flags_header.setSectionResizeMode(
            1, QHeaderView.ResizeToContents)
