watch:
	dotnet watch run

migrate $(name):
	@echo "Migrating $(name)..."
	dotnet ef migrations add $(name)
	dotnet ef database update