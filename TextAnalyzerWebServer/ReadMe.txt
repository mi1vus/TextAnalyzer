Локальная версия программы: TextAnalyzer, использует базу данных MSSQL, строка
подключения к которой находится в App.config файле под именем Test.
Допустимые параметры, передаваемые при старте, имеют следующий вид:

	-add "C:\Temp\dictionaryes\dictionary1.txt"
	-update "C:\Temp\dictionaryes\dictionary.txt"
	-clear

Путь до файла указывается в кавычках!
Запуск без параметров переводит программу в режим автодополнения ввода 
пользователя из уже имеющегося словаря в базе данных.

Серверная версия программы автодополнения ввода: TextAnalyzerWebServer, 
использует базу данных MS SQL, строка подключения к которой передается в 
параметрах при старте программы. Допустимые параметры, передаваемые при старте, 
имеют следующий вид:

	-connection "Server=.\\SQLEXP; Database=TextAnalyzer3; User Id=sa;Password=3108020718;" -port 1234
	-connection "Server=(localdb)\mssqllocaldb; Database=TextAnalyzer3; Trusted_Connection=True;" -p 1234
	-c "Server=PCMAX\SQLEXP; Database=TextAnalyzer3; User Id=sa;Password=3108020718;" -p 1234

Строка подключения указывается в кавычках!
Во время работы сервера допускаются следующие команды:

	-add "C:\Temp\dictionaryes\dictionary1.txt"
	-update "C:\Temp\dictionaryes\dictionary.txt"
	-clear

Путь до файла указывается в кавычках!
Клиентская версия программы автодополнения ввода:TextAnalyzerWebClient, 
подключается к серверу по указанному при запуске хосту и порту.
Допустимые параметры, передаваемые при старте, 
имеют следующий вид:

	-address 192.168.1.144 -port 1234
	-a 192.168.1.144 -p 1234