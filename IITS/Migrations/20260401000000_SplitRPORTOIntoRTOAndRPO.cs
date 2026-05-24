using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IITS.Migrations
{
    /// <summary>
    /// Separa el campo RPORTO (combinado) en dos campos distintos: RTO y RPO.
    /// Cumple con ISO-067-GCS que exige que Recovery Time Objective y Recovery Point Objective
    /// se definan por separado para cada aplicación y servicio.
    /// Los datos existentes en RPORTO se preservan en ambos campos para revisión manual posterior.
    /// </summary>
    public partial class SplitRPORTOIntoRTOAndRPO : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Aplicaciones
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Aplicaciones') AND name = 'RTO')
    ALTER TABLE [Aplicaciones] ADD [RTO] nvarchar(100) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Aplicaciones') AND name = 'RPO')
    ALTER TABLE [Aplicaciones] ADD [RPO] nvarchar(100) NULL;
-- Migrar datos: copiar el valor combinado a ambos campos para revisión manual
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Aplicaciones') AND name = 'RPORTO')
BEGIN
    UPDATE [Aplicaciones]
    SET [RTO] = LEFT(ISNULL([RPORTO], ''), 100),
        [RPO] = LEFT(ISNULL([RPORTO], ''), 100)
    WHERE [RPORTO] IS NOT NULL AND ([RTO] IS NULL OR [RPO] IS NULL);
END
");

            // Operaciones
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Operaciones') AND name = 'RTO')
    ALTER TABLE [Operaciones] ADD [RTO] nvarchar(100) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Operaciones') AND name = 'RPO')
    ALTER TABLE [Operaciones] ADD [RPO] nvarchar(100) NULL;
-- Migrar datos: copiar el valor combinado a ambos campos para revisión manual
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Operaciones') AND name = 'RPORTO')
BEGIN
    UPDATE [Operaciones]
    SET [RTO] = LEFT(ISNULL([RPORTO], ''), 100),
        [RPO] = LEFT(ISNULL([RPORTO], ''), 100)
    WHERE [RPORTO] IS NOT NULL AND ([RTO] IS NULL OR [RPO] IS NULL);
END
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Restaurar RPORTO si se hace rollback
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Aplicaciones') AND name = 'RPORTO')
    ALTER TABLE [Aplicaciones] ADD [RPORTO] nvarchar(200) NULL;
UPDATE [Aplicaciones] SET [RPORTO] = COALESCE([RTO], [RPO]) WHERE [RPORTO] IS NULL AND ([RTO] IS NOT NULL OR [RPO] IS NOT NULL);

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Operaciones') AND name = 'RPORTO')
    ALTER TABLE [Operaciones] ADD [RPORTO] nvarchar(200) NULL;
UPDATE [Operaciones] SET [RPORTO] = COALESCE([RTO], [RPO]) WHERE [RPORTO] IS NULL AND ([RTO] IS NOT NULL OR [RPO] IS NOT NULL);
");

            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Aplicaciones') AND name = 'RTO')
    ALTER TABLE [Aplicaciones] DROP COLUMN [RTO];
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Aplicaciones') AND name = 'RPO')
    ALTER TABLE [Aplicaciones] DROP COLUMN [RPO];

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Operaciones') AND name = 'RTO')
    ALTER TABLE [Operaciones] DROP COLUMN [RTO];
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Operaciones') AND name = 'RPO')
    ALTER TABLE [Operaciones] DROP COLUMN [RPO];
");
        }
    }
}
