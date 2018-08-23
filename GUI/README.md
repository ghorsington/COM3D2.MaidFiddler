# Maid Fiddler GUI

This is a GUI for Maid Fiddler Core. The GUI is made in PyQt (for various reasons -- one of them being the chance to learn myself Python ;) )

This is a **standalone** application that is run when COM3D2 is running.

## Beta stage

Note that this GUI is still in **beta**. All main features are complete, but not everything is bug-free or final.


## Running the tool

If you are a developer, you can run the GUI without building it.

### Requirements

* Python 3.6+
* [Universal CRT](https://support.microsoft.com/fi-fi/help/2999226/update-for-universal-c-runtime-in-windows)
    - Mainly, the following DLLs are needed:  

        ```
        api-ms-win-crt-convert-l1-1-0.dll
        api-ms-win-crt-environment-l1-1-0.dll
        api-ms-win-crt-filesystem-l1-1-0.dll
        api-ms-win-crt-heap-l1-1-0.dll
        api-ms-win-crt-locale-l1-1-0.dll
        api-ms-win-crt-math-l1-1-0.dll
        api-ms-win-crt-runtime-l1-1-0.dll
        api-ms-win-crt-stdio-l1-1-0.dll
        api-ms-win-crt-string-l1-1-0.dll
        api-ms-win-crt-time-l1-1-0.dll
        api-ms-win-crt-utility-l1-1-0.dll
        ```

Do the following:

1. Install **Python 3.6**

2. Clone this repository (if you haven't done so yet) by running

    ```bash
    git clone https://github.com/denikson/COM3D2.MaidFiddler.git
    ```

3. [Optional] Open command prompt in `GUI` folder and set up a virtual environment:

    ```bash
    py venv venv
    ```

    Activate the environment by running `venv/Scripts/activate.bat` in the same command prompt

4. Install required dependencies via pip:

    ```bash
    py -m pip install -r requirements.txt
    ```

    If you have set up a virtual environment, the packages will be installed there


5. Run the GUI via

    ```bash
    py app.py
    ```