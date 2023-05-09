try { $OutputEncoding = [console]::InputEncoding = [console]::OutputEncoding = New-Object System.Text.UTF8Encoding; } catch{} # Encoding 

clear

# Get the location of this script ps1
$script_dir = [io.path]::GetDirectoryName($PSCommandPath)  

# Get all *.cs recursively in $script_dir
$cs_files = gci -Recurse -Filter *.cs $script_dir

$regex_initial_filter_str = '(^.*?($|\{|\;)|.*$)'
$regex_initial_filter = New-Object System.Text.RegularExpressions.Regex($regex_initial_filter_str, ([System.Text.RegularExpressions.RegexOptions]::IgnoreCase + [System.Text.RegularExpressions.RegexOptions]::Multiline + [System.Text.RegularExpressions.RegexOptions]::Global))

# Get all methods
$regex_str = '(namespace|public|private|class)(\s|\t)+[a-zA-Z]+[^$\{\;]+(\s\t)?[^{$;]*[^{$;]*'
# Case insensitive
$regex = New-Object System.Text.RegularExpressions.Regex($regex_str, [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)

write-host "--- START CLASSES ---"
# Get all methods
$cs_files | % {
    $file = $_.FullName
    $file_content = gc $file -Encoding UTF8

    $file_content_no_comments = ($file_content -join "`n") -replace '(?ms)(/\*.*\*/|//[^\n]*$)',''
    $file_content_split_bodies = (($regex_initial_filter.Matches($file_content_no_comments) |%{$_.Value.ToString()}) -Join "`n")
    $file_content_no_properties = ($file_content_split_bodies -join "`n") -replace '(?ms)((public|private)\s+[^;{}]*{\s*(get|set)\s*;)[^}]+}',''
    $file_content_no_annotations = ($file_content_no_properties -join "`n") -replace '\[[^$\]]+]',''

    $matches = $regex.Matches($file_content_no_annotations -Join "`n")
    if($matches.Count -ge 1) {
        write-host ("=== " + ($_.FullName.Replace($script_dir,"").TrimStart('\')) + " ===")
        $first = $true
        $matches | % {
            if($_.Value.Contains("class")) {
                if(-not $first) {
                    #write-host ""
                }
                Write-Host ($_.Value.Trim() + ":")
            }
            elseif($_.Value.Contains("namespace")) {
                Write-Host ("[" + $_.Value.Trim() + "]")
            }
            else {
                Write-Host ("`t* " + $_.Value.Trim())
                $first = $false
            }
        }
    write-host ""
    }
}
write-host "--- END CLASSES ---"