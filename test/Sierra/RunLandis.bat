rem Batch File to Run a Scenario 
rem The 'rem' keyword indicates that this is a remark

rem call landis-ii-8 scenario_0.5_.txt
rem MOVE outputs\density outputs\density_0.5_.csv

rem call landis-ii-8 scenario_0.6_.txt
rem MOVE outputs\density outputs\density_0.6_
 
call landis-ii-8 scenario_0.7_.txt
rem MOVE outputs\density outputs\density_0.7_.csv
 
rem call landis-ii-8 scenario_0.8_.txt
rem MOVE outputs\density outputs\density_0.8_.csv
 
rem call landis-ii-8 scenario_0.9_.txt
rem MOVE outputs\density outputs\density_0.9_.csv
 
rem call landis-ii-8 scenario_1_.txt
rem MOVE outputs\density outputs\density_1.0_.csv

rem Always use the 'call' keyword before invoking landis-ii in a batch file.
rem The call keyword is necessary because the landis-ii command is itself a batch file.

pause

rem Add a pause so that you can assess whether the scenario ran to completion or whether 
rem it encountered input parameter errors or any other error.

