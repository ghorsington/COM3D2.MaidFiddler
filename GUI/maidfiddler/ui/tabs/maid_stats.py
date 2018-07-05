from PyQt5.QtWidgets import QHeaderView, QTableWidgetItem, QLineEdit, QCheckBox, QWidget, QHBoxLayout
from PyQt5.QtCore import Qt, QObject
from .ui_tab import UiTab


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
