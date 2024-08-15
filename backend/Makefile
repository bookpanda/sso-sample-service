watch:
	dotnet watch run

migrate $(name):
	@echo "Migrating $(name)..."
	dotnet ef migrations add $(name)
	dotnet ef database update

publish:
	dotnet publish "backend.csproj" -c Release -o ./publish