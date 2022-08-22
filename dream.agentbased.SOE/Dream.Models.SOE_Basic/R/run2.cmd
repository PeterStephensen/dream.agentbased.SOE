echo off
time /t

for /l %%i in (1 1 30) do (
	for /l %%x in (1 1 5) do (
		echo %%i %%x
		start ..\bin\Debug\net6.0\Dream.Models.SOE_Basic
        	ping 127.0.0.1 -n 4 > nul 
		start ..\bin\Debug\net6.0\Dream.Models.SOE_Basic 1
        	ping 127.0.0.1 -n 4 > nul 
		start ..\bin\Debug\net6.0\Dream.Models.SOE_Basic 2
        	ping 127.0.0.1 -n 4 > nul 
	)
       	ping 127.0.0.1 -n 150 > nul 
)

time /t
pause