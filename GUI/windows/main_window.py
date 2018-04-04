from util.eventpoller import EventPoller
import util.util as util
import base64
import PyQt5.uic as uic
from PyQt5.QtCore import Qt
from PyQt5.QtGui import QPixmap, QIcon
from PyQt5.QtWidgets import QHeaderView, QTableWidgetItem, QLineEdit, QCheckBox, QSpinBox, QWidget, QHBoxLayout, QListWidgetItem

class MainWindow:
    def __init__(self, core, group):
        self.ui = uic.loadUi(open(util.get_resource_path("templates/maid_fiddler.ui")))
        self.core = core
        self.event_poller = EventPoller(8890)
        self.event_poller.start(self.core, group)
        self.load_ui()

    def close(self):
        self.event_poller.stop()

    def __init_maid_info(self):
        """ Init maid info"""

        for personal in self.game_data["personal_list"]:
            self.ui.personality_combo.addItem(personal["name"], personal["id"])

        for (contract_name, contract_id) in self.game_data["contract"].items():
            self.ui.contract_combo.addItem(contract_name, contract_id)

        for (relation_name, relation_id) in self.game_data["relation"].items():
            self.ui.relation_combo.addItem(relation_name, relation_id)

        for (seik_name, seik_id) in self.game_data["seikeiken"].items():
            self.ui.current_combo.addItem(seik_name, seik_id)
            self.ui.initial_combo.addItem(seik_name, seik_id)

        for job_class in self.game_data["job_class_list"]:
            self.ui.job_class_combo.addItem(job_class["name"], job_class["id"])

        for yotogi_class in self.game_data["yotogi_class_list"]:
            self.ui.yotogi_class_combo.addItem(
                yotogi_class["name"], yotogi_class["id"])

        self.ui.yotogi_class_combo.view().setVerticalScrollBarPolicy(Qt.ScrollBarAsNeeded)

    def __init_maid_stats_tab(self):
        """Init maid stats tab."""

        self.ui.maid_params_lockable_table.horizontalHeader(
        ).setSectionResizeMode(0, QHeaderView.Stretch)
        self.ui.maid_params_lockable_table.horizontalHeader(
        ).setSectionResizeMode(1, QHeaderView.ResizeToContents)
        self.ui.maid_params_lockable_table.horizontalHeader(
        ).setSectionResizeMode(2, QHeaderView.ResizeToContents)

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

        self.ui.maid_params_bonus_table.horizontalHeader().setSectionResizeMode(0,
                                                                                    QHeaderView.Stretch)
        self.ui.maid_params_bonus_table.horizontalHeader(
        ).setSectionResizeMode(1, QHeaderView.ResizeToContents)

        self.ui.maid_params_bonus_table.setRowCount(
            len(self.game_data["maid_bonus_status"]))

        for (i, maid_prop) in enumerate(self.game_data["maid_bonus_status"]):
            name = QTableWidgetItem(maid_prop)
            line = QLineEdit()
            line.setStyleSheet("width: 15em;")

            self.ui.maid_params_bonus_table.setItem(i, 0, name)
            self.ui.maid_params_bonus_table.setCellWidget(i, 1, line)

    def __init_feature_propensity_tab(self):
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

    def __init_yotogi_tab(self):
        self.ui.yotogi_skills_table.setRowCount(
            len(self.game_data["yotogi_skills"]))

        yotogi_skills_header = self.ui.yotogi_skills_table.horizontalHeader()

        yotogi_skills_header.setSectionResizeMode(0, QHeaderView.ResizeToContents)
        yotogi_skills_header.setSectionResizeMode(1, QHeaderView.Stretch)
        yotogi_skills_header.setSectionResizeMode(2, QHeaderView.ResizeToContents)
        yotogi_skills_header.setSectionResizeMode(3, QHeaderView.ResizeToContents)
        yotogi_skills_header.setSectionResizeMode(4, QHeaderView.ResizeToContents)

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

    def __init_work_tab(self):
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

        yotogi_work_header.setSectionResizeMode(0, QHeaderView.ResizeToContents)
        yotogi_work_header.setSectionResizeMode(1, QHeaderView.Stretch)
        yotogi_work_header.setSectionResizeMode(2, QHeaderView.ResizeToContents)
        yotogi_work_header.setSectionResizeMode(3, QHeaderView.ResizeToContents)

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

    def __init_player_tab(self):
        self.ui.player_params_table.setRowCount(
            len(self.game_data["club_status_settable"]))

        player_params_header = self.ui.player_params_table.horizontalHeader()

        player_params_header.setSectionResizeMode(0, QHeaderView.Stretch)
        player_params_header.setSectionResizeMode(2, QHeaderView.ResizeToContents)
        player_params_header.setSectionResizeMode(3, QHeaderView.ResizeToContents)

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
        player_flags_header.setSectionResizeMode(1, QHeaderView.ResizeToContents)

    def load_ui(self):
        self.game_data = self.core.GetGameInfo()

        self.__init_maid_info()
        self.__init_maid_stats_tab()
        self.__init_feature_propensity_tab()
        self.__init_yotogi_tab()
        self.__init_work_tab()
        self.__init_player_tab()

        # Maid list test

        self.event_poller.on("maid_prop_changed", lambda args: print(str(args)))

        maid_data = self.core.GetMaidData("3ac")

        thumb_image = base64.b64decode(maid_data["maid_thumbnail"])

        thumb = QPixmap()
        thumb.loadFromData(thumb_image)

        self.ui.maid_list.addItem(QListWidgetItem(QIcon(thumb), "%s %s" % (
            maid_data["set_properties"]["firstName"], maid_data["set_properties"]["lastName"])))
