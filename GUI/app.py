import sys
import os
import zerorpc
import PyQt5.uic as uic
from PyQt5.QtCore import Qt
from PyQt5.QtWidgets import QApplication, QMainWindow, QStyleFactory, QSizePolicy, QHeaderView, QTableWidgetItem, QLineEdit, QCheckBox, QWidget, QHBoxLayout, QListWidgetItem

ui_template = "maid_fiddler.ui"

if getattr(sys, "_MEIPASS", False):
    ui_template = os.path.join(getattr(sys, "_MEIPASS"), ui_template)

client = None
main_window = None

def connect():
    global client

    print("Connecting to tcp://127.0.0.1:8899")
    client = zerorpc.Client()
    client.connect("tcp://127.0.0.1:8899")
    try:
        client._zerorpc_ping()
    except Exception as ex:
        print("Failed to connect because " + str(ex))
    print("Connected!")

def init_main_ui():
    global main_window
    main_window = uic.loadUi(open(ui_template))

    game_data = client.GetGameInfo()

    for personal in game_data["personal_list"]:
        main_window.personality_combo.addItem(personal["name"], personal["id"])

    for (contract_name, contract_id)  in game_data["contract"].items():
        main_window.contract_combo.addItem(contract_name, contract_id)

    for (relation_name, relation_id)  in game_data["relation"].items():
        main_window.relation_combo.addItem(relation_name, relation_id)

    for (seik_name, seik_id)  in game_data["seikeiken"].items():
        main_window.current_combo.addItem(seik_name, seik_id)
        main_window.initial_combo.addItem(seik_name, seik_id)

    for job_class in game_data["job_class_list"]:
        main_window.job_class_combo.addItem(job_class["name"], job_class["id"])

    for yotogi_class in game_data["yotogi_class_list"]:
        main_window.yotogi_class_combo.addItem(yotogi_class["name"], yotogi_class["id"])

    main_window.yotogi_class_combo.view().setVerticalScrollBarPolicy(Qt.ScrollBarAsNeeded)

    # Maid base stats table

    main_window.maid_params_lockable_table.horizontalHeader().setSectionResizeMode(0, QHeaderView.Stretch)
    main_window.maid_params_lockable_table.horizontalHeader().setSectionResizeMode(1, QHeaderView.ResizeToContents)
    main_window.maid_params_lockable_table.horizontalHeader().setSectionResizeMode(2, QHeaderView.ResizeToContents)

    main_window.maid_params_lockable_table.setRowCount(len(game_data["maid_status_settable"]))

    for (i, maid_prop) in enumerate(game_data["maid_status_settable"]):
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

        main_window.maid_params_lockable_table.setItem(i, 0, name)
        main_window.maid_params_lockable_table.setCellWidget(i, 1, line)
        main_window.maid_params_lockable_table.setCellWidget(i, 2, widget)

    # Maid bonus stats table

    main_window.maid_params_bonus_table.horizontalHeader().setSectionResizeMode(0, QHeaderView.Stretch)
    main_window.maid_params_bonus_table.horizontalHeader().setSectionResizeMode(1, QHeaderView.ResizeToContents)

    main_window.maid_params_bonus_table.setRowCount(len(game_data["maid_bonus_status"]))

    for (i, maid_prop) in enumerate(game_data["maid_bonus_status"]):
        name = QTableWidgetItem(maid_prop)
        line = QLineEdit()
        line.setStyleSheet("width: 15em;")

        main_window.maid_params_bonus_table.setItem(i, 0, name)
        main_window.maid_params_bonus_table.setCellWidget(i, 1, line)

    # Feature list

    for feature in game_data["feature_list"]:
        item = QListWidgetItem(feature["name"])
        item.setData(Qt.UserRole, feature["id"])
        item.setFlags(item.flags() | Qt.ItemIsUserCheckable)
        item.setCheckState(Qt.Unchecked)
        main_window.feature_list.addItem(item)

    main_window.feature_list.setMinimumHeight(main_window.feature_list.sizeHintForRow(0) * (len(game_data["feature_list"]) + 1))

    # Propensity list

    for propensity in game_data["propensity_list"]:
        item = QListWidgetItem(propensity["name"])
        item.setData(Qt.UserRole, propensity["id"])
        item.setFlags(item.flags() | Qt.ItemIsUserCheckable)
        item.setCheckState(Qt.Unchecked)
        main_window.propensity_list.addItem(item)

    main_window.propensity_list.setMinimumHeight(main_window.propensity_list.sizeHintForRow(0) * (len(game_data["propensity_list"]) + 1))

    game_data = client.GetGameInfo()

def main():
    

    print("Starting MF")
    connect()
    app = QApplication(sys.argv)
    app.setStyle(QStyleFactory.create("Fusion"))

    init_main_ui()
    
    main_window.show()
    sys.exit(app.exec_())

if __name__ == "__main__":
    main()
