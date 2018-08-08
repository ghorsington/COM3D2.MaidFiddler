from PyQt5.QtWidgets import QHeaderView, QTableWidgetItem, QCheckBox, QSpinBox, QWidget, QHBoxLayout, QGroupBox, QLabel
from PyQt5.QtCore import Qt
from .ui_tab import UiTab
from maidfiddler.ui.qt_elements import NumberElement
from maidfiddler.util.translation import tr, tr_str


class WorkTab(UiTab):
    def __init__(self, ui):
        UiTab.__init__(self, ui)

        self.work_elements = {}
        self.noon_work_id_index = {}
        self.night_work_id_index = {}

        self.work_day_names = []
        self.work_yotogi_names = []

    def update_ui(self):
        self.work_elements.clear()
        self.night_work_id_index.clear()
        self.noon_work_id_index.clear()
        self.work_day_names.clear()
        self.work_yotogi_names.clear()

        self.ui.cur_noon_work_combo.blockSignals(True)
        self.ui.cur_night_work_combo.blockSignals(True)
        self.ui.noon_work_table.clearContents()
        self.ui.cur_noon_work_combo.clear()
        self.ui.cur_night_work_combo.clear()

        noon_work = [data for data in self.game_data["work_data"]
                     if data["work_type"] != "Yotogi"]

        self.ui.noon_work_table.setRowCount(len(noon_work))

        noon_work_header = self.ui.noon_work_table.horizontalHeader()

        noon_work_header.setSectionResizeMode(0, QHeaderView.Stretch)
        noon_work_header.setSectionResizeMode(1, QHeaderView.ResizeToContents)
        noon_work_header.setSectionResizeMode(2, QHeaderView.ResizeToContents)

        for (i, work_data) in enumerate(noon_work):
            self.ui.cur_noon_work_combo.addItem(work_data["name"], work_data["id"])
            self.work_day_names.append(f"work_noon.{work_data['name']}")
            self.noon_work_id_index[work_data["id"]] = i

            name = QTableWidgetItem(work_data["name"])
            name.setWhatsThis(f"work_noon.{work_data['name']}")
            line_level = QSpinBox()
            line_level.setMinimum(0)
            line_level.setProperty("work_id", work_data["id"])
            line_play_count = QSpinBox()
            line_play_count.setMinimum(0)
            line_play_count.setProperty("work_id", work_data["id"])

            level = NumberElement(line_level)
            play_count = NumberElement(line_play_count)
            self.work_elements[work_data["id"]] = (level, play_count)

            level.connect(self.change_level)
            play_count.connect(self.change_play_count)

            self.ui.noon_work_table.setItem(i, 0, name)
            self.ui.noon_work_table.setCellWidget(i, 1, line_level)
            self.ui.noon_work_table.setCellWidget(i, 2, line_play_count)

        # Yotogi work

        yotogi_work = [data for data in self.game_data["work_data"]
                       if data["work_type"] == "Yotogi"]

        for (i, work_data) in enumerate(yotogi_work):
            self.ui.cur_night_work_combo.addItem(work_data["name"], work_data["id"])
            self.night_work_id_index[work_data["id"]] = i
            self.work_yotogi_names.append(f"work_yotogi.{work_data['name']}")

        self.ui.cur_noon_work_combo.blockSignals(False)
        self.ui.cur_night_work_combo.blockSignals(False)

    def init_events(self, event_poller):
        self.ui.cur_noon_work_combo.currentIndexChanged.connect(lambda: self.core.SetNoonWorkActive(self.ui.cur_noon_work_combo.currentData(Qt.UserRole)))
        self.ui.cur_night_work_combo.currentIndexChanged.connect(lambda: self.core.SetNightWorkActive(self.ui.cur_night_work_combo.currentData(Qt.UserRole)))
        event_poller.on("work_data_changed", self.work_data_changed)

    def work_data_changed(self, args):
        (level, play_count) = self.work_elements[args["id"]]
        level.set_value(args["level"])
        play_count.set_value(args["play_count"])

    def change_level(self):
        level = self.sender()
        self.core.SetWorkLevelActiveMaid(level.property("work_id"), level.value())

    def change_play_count(self):
        count = self.sender()
        self.core.SetWorkPlayCountActive(count.property("work_id"), count.value())

    def on_maid_selected(self):
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

        self.ui.cur_noon_work_combo.blockSignals(True)
        if maid["properties"]["active_noon_work_id"] in self.noon_work_id_index:
            self.ui.cur_noon_work_combo.setCurrentIndex(self.noon_work_id_index[maid["properties"]["active_noon_work_id"]])
        self.ui.cur_noon_work_combo.blockSignals(False)

        self.ui.cur_night_work_combo.blockSignals(True)
        if maid["properties"]["active_night_work_id"] in self.night_work_id_index:
            self.ui.cur_night_work_combo.setCurrentIndex(self.night_work_id_index[maid["properties"]["active_night_work_id"]])
        self.ui.cur_night_work_combo.blockSignals(False)
    
    def translate_ui(self):
        self.ui.ui_tabs.setTabText(3, tr(self.ui.tab_maid_work))

        for group in self.ui.tab_maid_work.findChildren(QGroupBox):
            group.setTitle(tr(group))

        for label in self.ui.tab_maid_work.findChildren(QLabel):
            label.setText(tr(label))

        for i, work_name in enumerate(self.work_day_names):
            self.ui.cur_noon_work_combo.setItemText(i, tr_str(work_name))

        for i, work_name in enumerate(self.work_yotogi_names):
            self.ui.cur_night_work_combo.setItemText(i, tr_str(work_name))

        for col in range(0, self.ui.noon_work_table.columnCount()):
            name = self.ui.noon_work_table.horizontalHeaderItem(col)
            name.setText(tr(name))

        for row in range(0, self.ui.noon_work_table.rowCount()):
            name = self.ui.noon_work_table.item(row, 0)
            name.setText(tr(name))