echo off

for /l %%x in (1 1 50) do (
	echo %%x
	..\bin\Debug\net6.0\Dream.Models.SOE_Basic
	..\bin\Debug\net6.0\Dream.Models.SOE_Basic 1
	..\bin\Debug\net6.0\Dream.Models.SOE_Basic 2
	..\bin\Debug\net6.0\Dream.Models.SOE_Basic 3
)
