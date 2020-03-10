
TextAnalyzer
	-add "C:\Temp\dictionaryes\dictionary1.txt"
	-update "C:\Temp\dictionaryes\dictionary.txt"
	-clear

TextAnalyzerWebServer
	-connection "Data Source=.\\SQLEXP; Initial Catalog=TextAnalyzer1; Integrated Security=true; MultipleActiveResultSets=true;" -port 1234
	-connection "Server=(localdb)\mssqllocaldb; Database=TextAnalyzer3; Trusted_Connection=True;" -p 1234
	-connection "Server=PCMAX\SQLEXP; Database=TextAnalyzer3; User Id=sa;Password=3108020718;" -p 1234
console commands:
	-add "C:\Temp\dictionaryes\dictionary1.txt"
	-update "C:\Temp\dictionaryes\dictionary.txt"
	-clear

TextAnalyzerWebServer.exe -connection "Server=PCMAX\SQLEXP; Database=TextAnalyzer3; User Id=sa;Password=3108020718;" -p 1234

TextAnalyzerWebClient
	-address 192.168.1.144 -port 1234
	
TextAnalyzerWebClient.exe -address 192.168.1.144 -port 1234
