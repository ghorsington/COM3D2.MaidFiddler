from PyQt5.QtWidgets import QHeaderView, QTableWidgetItem, QCheckBox, QWidget, QHBoxLayout, QLineEdit
from PyQt5.QtCore import Qt
from .ui_tab import UiTab


class PlayerTab(UiTab):
    def update_ui(self):
        self.ui.player_params_table.setRowCount(
            len(self.game_data["club_status_settable"]))

        player_params_header = self.ui.player_params_table.horizontalHeader()

        player_params_header.setSectionResizeMode(0, QHeaderView.Stretch)
        player_params_header.setSectionResizeMode(
            1, QHeaderView.ResizeToContents)
        player_params_header.setSectionResizeMode(
            2, QHeaderView.ResizeToContents)

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
