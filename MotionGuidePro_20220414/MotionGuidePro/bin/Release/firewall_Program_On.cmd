@echo off
echo.
echo	:::: �������ȭ�� ��ǰ��̵� �ιٿ�� ����  ::::
pushd "%~dp0"
SET cvs_path=%cd%
echo %cvs_path%
netsh advfirewall firewall add rule name="CREVIS_MOTION" dir=in action=allow program="%cvs_path%\MotionGuidePro.exe" enable=yes
if errorlevel 1 (
	cls
	echo.
	echo �˼��մϴ�.������ϼ���.��Ŭ������ �����ڱ��� ������ �ʿ��մϴ�.
	pause>nul
	exit
)
echo ��ȭ�� �ιٿ���Ģ�� CREVIS_MOTION �̸����� ���ó���Ǿ����ϴ�. �ƹ�Ű�� �����ʽÿ�.
exit