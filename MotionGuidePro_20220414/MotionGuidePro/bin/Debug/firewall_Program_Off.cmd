@echo off
echo.
echo	:::: �������ȭ�� ��ǰ��̵� �ιٿ�� ����  ::::
pushd "%~dp0"
SET cvs_path=%cd%
echo %cvs_path%
netsh advfirewall firewall delete rule name="CREVIS_MOTION" program="%cvs_path%\MotionGuidePro.exe"
if errorlevel 1 (
	cls
	echo.
	echo �˼��մϴ�.������ϼ���.��Ŭ������ �����ڱ��� ������ �ʿ��մϴ�.
	pause>nul
	exit
)
echo ��ȭ�� �ιٿ���Ģ�� ���� �߽��ϴ�.
pause>nul
exit