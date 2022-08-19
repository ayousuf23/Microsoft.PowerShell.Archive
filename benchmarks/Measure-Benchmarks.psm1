function Measure-These {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)] $Count,
        [Parameter(Mandatory = $false)] [ScriptBlock] $BeforeAll,
        [Parameter(Mandatory = $false)] [ScriptBlock] $BeforeEach,
        [Parameter(Mandatory = $true)] [ScriptBlock[]] $ToMeasure,
        [Parameter(Mandatory = $true)] [string[]] $Titles,
        [Parameter(Mandatory = $false)] [ScriptBlock] $AfterEach
    )
    begin {
        $results = @()
    }
    process {
        if ($BeforeAll -ne $null) {
            Invoke-Command $BeforeAll -ErrorAction Stop
        } 

        $scriptBlockCount = 0

        1..$ToMeasure.Count | % {
            
            $totalTime = 0
            $minTime = -1
            $maxTime = -1

            1..$Count | % {
                # Run the before block
                if ($null -ne $BeforeEach) {
                    Invoke-Command $BeforeEach -ErrorAction Stop
                }
                # Run the block to measure
                $timeTaken = Measure-Command -Expression $ToMeasure[$scriptBlockCount]

                $totalTime += $timeTaken.TotalMilliseconds

                if ($minTime -lt 0) {
                    $minTime = $timeTaken.TotalMilliseconds
                } else {
                    $minTime = [Math]::Min($minTime, $timeTaken.TotalMilliseconds)
                }

                if ($maxTime -lt 0) {
                    $maxTime = $timeTaken.TotalMilliseconds
                } else {
                    $maxTime = [Math]::Max($minTime, $timeTaken.TotalMilliseconds)
                }

                # Run the after block
                if ($null -ne $AfterEach) {
                    Invoke-Command $AfterEach -ErrorAction Stop
                } 
            }

            
            $avgTime = $totalTime / $Count

            $results += [PSCustomObject]@{
                Average = $avgTime
                Minimum = $minTime
                Maximum = $maxTime
                Total = $totalTime
                Title = $Titles[$scriptBlockCount]
            }

            $scriptBlockCount++
        }
        
    }
    end {

        return $results
    }
}