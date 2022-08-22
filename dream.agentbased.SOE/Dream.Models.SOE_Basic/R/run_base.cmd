echo off

for /l %%i in (1 1 5) do (
	for /l %%x in (1 1 15) do (
		echo %%i %%x
		start ..\bin\Debug\net6.0\Dream.Models.SOE_Basic
        	ping 127.0.0.1 -n 2 > nul 
        	
	)
       	ping 127.0.0.1 -n 190 > nul 
)

pause