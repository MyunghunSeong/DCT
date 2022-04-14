@echo off
echo.
echo	:::: 윈도우방화벽 모션가이드 인바운드 설정  ::::
pushd "%~dp0"
SET cvs_path=%cd%
echo %cvs_path%
netsh advfirewall firewall delete rule name="CREVIS_MOTION" program="%cvs_path%\MotionGuidePro.exe"
if errorlevel 1 (
	cls
	echo.
	echo 죄송합니다.재실행하세요.우클릭으로 관리자권한 실행이 필요합니다.
	pause>nul
	exit
)
echo 방화벽 인바운드규칙을 해제 했습니다.
pause>nul
exit