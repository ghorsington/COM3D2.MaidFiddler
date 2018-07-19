from PyQt5.QtWidgets import QHeaderView, QTableWidgetItem, QCheckBox, QSpinBox, QWidget, QHBoxLayout
from PyQt5.QtCore import Qt
from .ui_tab import UiTab
from maidfiddler.ui.qt_elements import NumberElement


class WorkTab(UiTab):
    def __init__(self, ui, core, maid_mgr):
        UiTab.__init__(self, ui, core, maid_mgr)

        self.work_elements = {}

    def update_ui(self):
        self.work_elements.clear()

        noon_work = [data for data in self.game_data["work_data"]
                     if data["work_type"] != "Yotogi"]

        self.ui.noon_work_table.setRowCount(len(noon_work))

        noon_work_header = self.ui.noon_work_table.horizontalHeader()

        noon_work_header.setSectionResizeMode(0, QHeaderView.Stretch)
        noon_work_header.setSectionResizeMode(1, QHeaderView.ResizeToContents)
        noon_work_header.setSectionResizeMode(2, QHeaderView.ResizeToContents)

        for (i, work_data) in enumerate(noon_work):
            self.ui.cur_noon_work_combo.addItem(
                work_data["name"], work_data["id"])

            name = QTableWidgetItem(work_data["name"])
            line_level = QSpinBox()
            line_level.setMinimum(0)
            line_play_count = QSpinBox()
            line_play_count.setMinimum(0)

            level = NumberElement(line_level)
            play_count = NumberElement(line_play_count)
            self.work_elements[work_data["id"]] = (level, play_count)

            self.ui.noon_work_table.setItem(i, 0, name)
            self.ui.noon_work_table.setCellWidget(i, 1, line_level)
            self.ui.noon_work_table.setCellWidget(i, 2, line_play_count)

        # Yotogi work

        yotogi_work = [data for data in self.game_data["work_data"]
                       if data["work_type"] == "Yotogi"]

        for (i, work_data) in enumerate(yotogi_work):
            self.ui.cur_night_work_combo.addItem(
                work_data["name"], work_data["id"])

    def init_events(self, event_poller):
        self.ui.maid_list.currentItemChanged.connect(self.maid_selected)

        self.ui.cur_noon_work_combo.currentIndexChanged.connect(lambda: self.core.SetNoonWorkActive(self.ui.cur_noon_work_combo.currentData(Qt.UserRole)))
        self.ui.cur_night_work_combo.currentIndexChanged.connect(lambda: self.core.SetNightWorkActive(self.ui.cur_noon_work_combo.currentData(Qt.UserRole)))

        for (level, play_count) in self.work_elements.values():
            level.connect(self.change_level)
            play_count.connect(self.change_play_count)

        event_poller.on("work_data_changed", self.work_data_changed)
    
    def work_data_changed(self, args):
        (level, play_count) = self.work_elements[args["id"]]
        level.set_value(args["level"])
        play_count.set_value(args["play_count"])

    def change_level(self):
        level = self.sender()
        self.core.SetWorkLevelActiveMaid(level.data(Qt.UserRole), level.value())

    def change_play_count(self):
        count = self.sender()
        self.core.SetWorkPlayCountActive(count.data(Qt.UserRole), count.value())

    def maid_selected(self):
        if self.maid_mgr.selected_maid is None:
            return

        maid = self.maid_mgr.selected_maid

        for work_id, (level, play_count) in self.work_elements.items():
            if work_id in maid["work_levels"]:
                level.set_value(maid["work_levels"][work_id])
                play_count.set_value(maid["work_play_counts"][work_id])
            else:
                level.set_value(0)
                play_count.set_value(0)

