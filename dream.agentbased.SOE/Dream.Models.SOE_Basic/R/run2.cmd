echo off
time /t

for /l %%i in (1 1 38) do (
	for /l %%x in (1 1 4) do (
		echo %%i %%x
		start ..\bin\Debug\net6.0\Dream.Models.SOE_Basic
        	ping 127.0.0.1 -n 2 > nul 
		start ..\bin\Debug\net6.0\Dream.Models.SOE_Basic 1
        	ping 127.0.0.1 -n 2 > nul 
		start ..\bin\Debug\net6.0\Dream.Models.SOE_Basic 2
        	ping 127.0.0.1 -n 2 > nul 
		start ..\bin\Debug\net6.0\Dream.Models.SOE_Basic 3
        	ping 127.0.0.1 -n 2 > nul 
	)
       	ping 127.0.0.1 -n 730 > nul 
)

time /t
pause