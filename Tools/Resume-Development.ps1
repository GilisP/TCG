param([string]$ProjectPath = (Split-Path -Parent $PSScriptRoot))

$ErrorActionPreference = 'Stop'
$recordPath = Join-Path $ProjectPath 'Documentacao\Desenvolvimento'
$checkpoint = Get-Content -LiteralPath (Join-Path $recordPath 'checkpoint.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$tasks = Get-Content -LiteralPath (Join-Path $recordPath 'tasks.json') -Raw -Encoding UTF8 | ConvertFrom-Json
if (!$checkpoint.taskId -or !$checkpoint.status -or !$checkpoint.next -or !$tasks.Count) {
    throw 'Registro de retomada incompleto.'
}
if (@($tasks.taskId | Select-Object -Unique).Count -ne $tasks.Count) {
    throw 'IDs de tarefas repetidos.'
}
foreach ($task in $tasks) {
    if (!$task.objective -or !$task.owner -or !$task.scope -or !$task.doneWhen) {
        throw "Contrato incompleto: $($task.taskId)"
    }
    foreach ($dependency in $task.dependencies) {
        if ($dependency -notin $tasks.taskId) { throw "Dependência ausente: $dependency" }
    }
}

"Retomada: $($checkpoint.taskId) | $($checkpoint.status) | $($checkpoint.date)"
"Projeto: $($checkpoint.project)"
"Contrato: $($checkpoint.contract)"
"Próximo passo: $($checkpoint.next)"
foreach ($task in $tasks) {
    "[$($task.taskId)] $($task.status) | $($task.owner) | $($task.objective)"
    "  Escopo: $($task.scope -join ', ')"
    "  Conclusão: $($task.doneWhen)"
}
foreach ($limitation in $checkpoint.limitations) { "Limite: $limitation" }
if ($checkpoint.evidence) { "Evidências: $($checkpoint.evidence -join ', ')" }
