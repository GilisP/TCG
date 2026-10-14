# Correção de input dos botões — 2026-10-14

O log enviado pelo autor mostrou InvalidOperationException em TableView.Button: o destaque de pressionamento consultava UnityEngine.Input.GetMouseButton, incompatível com activeInputHandler=1 (Input System exclusivo). O problema se repetia ao passar o mouse sobre botões no menu.

Correção: apresentação usa GUIUtility.hotControl e Event.current.button, pertencentes ao IMGUI já usado pela interface. Sem alterar Input Handling, pacotes, regras ou perfil. Fundação não contém outras leituras UnityEngine.Input.

Verificação aprovada em Editor Play Mode: InputButtonChecks entra na Mesa em Play Mode, força hover em Button, visita as quatro páginas do hub e captura exceções. Resultado: PASS, 128 chamadas com hover forçado e quatro páginas, sem exceções. Evidência em Logs/InputButtonVerification/result.txt; log input-button-play-ui-20261014.log. Compilação do Editor aprovada. Executável anterior não foi reconstruído nesta correção, destinada ao projeto/Editor. Teste dedicado à causa enviada; não substitui as pendências anteriores de arraste/hover/pilhas.
