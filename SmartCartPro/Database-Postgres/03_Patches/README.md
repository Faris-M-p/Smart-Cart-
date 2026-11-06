# Baseline and future patches

`V001__Baseline.sql` is the reviewed pre-release baseline. It includes the
authoritative final definitions in `01_Tables/create.sql` and required system
data in `02_Seed/create.sql`, then records the V001 success marker in the same
transaction. It contains no dummy/demo records.

`BuildDatabase.bat` creates a new target database through a maintenance
connection, then installs V001 exactly once. If the target already exists, the
runner leaves it unchanged and directs you to `ApplyPatches.bat`. The patch
runner requires the exact baseline marker and applies future
`V002__Description.sql`, `V003__Description.sql`, and later patches. It skips
V001 because the build runner already installed it. It never reads
`06_DummyData/`.

Do not create an empty V002 now. Do not edit V001 after release or on a shared
database; add the next additive version instead. A pre-release development DB
with the old V001–V013 sequence requires the explicit backup/schema review and
history transition documented in `../README.md`; the runner rejects old V001
history rather than reinterpreting it.
