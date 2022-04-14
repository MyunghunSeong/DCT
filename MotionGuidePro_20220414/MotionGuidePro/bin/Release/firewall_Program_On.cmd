@echo off
echo.
echo	:::: 윈도우방화벽 모션가이드 인바운드 설정  ::::
pushd "%~dp0"
SET cvs_path=%cd%
echo %cvs_path%
netsh advfirewall firewall add rule name="CREVIS_MOTION" dir=in action=allow program="%cvs_path%\MotionGuidePro.exe" enable=yes
if errorlevel 1 (
	cls
	echo.
	echo 죄송합니다.재실행하세요.우클릭으로 관리자권한 실행이 필요합니다.
	pause>nul
	exit
)
echo 방화벽 인바운드규칙에 CREVIS_MOTION 이름으로 등록처리되었습니다. 아무키나 누르십시오.
exit