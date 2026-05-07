# Hастройка окружения
### Версия Unity
`6000.4.5f1`

### Настройка Git
Команды:
* `git config --local merge.tool unityyamlmerge`
* `git config --local mergetool.unityyamlmerge.trustExitCode true`
* Эта команда настроит работу с мерёж конфликтами на встроенный в Unity инструмент. Вам необходимо найти где он у вас лежит и вставить сюда соответствующий путь:
  ```
git config --local mergetool.unityyamlmerge.cmd\
"'Найдите ваш путь к Unity Hub/Editor/6000.4.5f1/Editor/Data/Tools/UnityYAMLMerge.exe' merge -p "$LOCAL\" \"$MERGED\""

  ```