pushd %~dp0
call build_constants.bat
%DB_METAL_EXE% --namespace:AsyncRPGSharedLib.Database --provider:SQLite --language:C# --code:%SHAREDLIB_SRC_DIR%\Database\LinqToSql.cs --culture:en --case:leave %SHAREDLIB_SRC_DIR%\Database\AsyncRPG.dbml
popd