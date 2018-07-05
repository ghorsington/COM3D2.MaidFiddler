from PyQt5.QtWidgets import QHeaderView, QTableWidgetItem, QCheckBox, QSpinBox, QWidget, QHBoxLayout
from PyQt5.QtCore import Qt
from .ui_tab import UiTab

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
