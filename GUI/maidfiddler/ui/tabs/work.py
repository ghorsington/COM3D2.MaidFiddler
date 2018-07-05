from PyQt5.QtWidgets import QHeaderView, QTableWidgetItem, QCheckBox, QSpinBox, QWidget, QHBoxLayout
from PyQt5.QtCore import Qt
from .ui_tab import UiTab


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
