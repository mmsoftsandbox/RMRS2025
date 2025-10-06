using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using RMRS.ConsoleApp.DataModel;

namespace RMRS.ConsoleApp.Context;

public partial class EmployeeDBContext : DbContext
{
    public EmployeeDBContext()
    {
    }

    public EmployeeDBContext(DbContextOptions<EmployeeDBContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Employee> Employees { get; set; }

#if false // При ининциалиации через DI испольуется явное конфигурирование
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        // Параметр connectionString заменён на String.Empty
        => optionsBuilder.UseSqlServer(String.Empty);
#endif

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.EmployeeId)
                .HasName("PK_Employees_EmployeeID")
                .IsClustered();

            entity.HasIndex(e => e.DateOfBirth, "IDX_Employees_DateOfBirth");

            entity.HasIndex(e => e.Email, "IDX_Employees_Email");

            entity.HasIndex(e => e.FirstName, "IDX_Employees_FirstName");

            entity.HasIndex(e => e.LastName, "IDX_Employees_LastName");

            entity.HasIndex(e => e.Salary, "IDX_Employees_Salary");

            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.DateOfBirth).HasColumnName("DateOfBirth");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("Email");
            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .HasColumnName("FirstName");
            entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .HasColumnName("LastName");
            entity.Property(e => e.Salary)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("Salary");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
