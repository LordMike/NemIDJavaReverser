SET KrakatauRoot="..\Krakatau"

python %KrakatauRoot%\disassemble.py -out disassembled DanID_Applet.jar
python %KrakatauRoot%\disassemble.py -out disassembled DanID_Applet-attachments.jar
python %KrakatauRoot%\disassemble.py -out disassembled DanID_Applet-boot-prod.jar
python %KrakatauRoot%\disassemble.py -out disassembled DanID_Applet-bouncycastle.jar
python %KrakatauRoot%\disassemble.py -out disassembled DanID_Applet-crypto.jar
python %KrakatauRoot%\disassemble.py -out disassembled DanID_Applet-nanoxml.jar
python %KrakatauRoot%\disassemble.py -out disassembled DanID_Applet-shortterm.jar

pause