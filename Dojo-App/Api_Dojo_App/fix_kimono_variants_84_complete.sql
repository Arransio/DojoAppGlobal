-- LIMPIEZA: Eliminar todas las variantes incorrectas
DELETE FROM ProductVariantColors;
DELETE FROM ProductVariants;

-- CREACIÓN: 84 variantes correctas
-- Estructura: Cada variante = 1 talla + 1 color primario + 1 color secundario
-- 4 tallas (A1, A2, A3, A4) × 3 colores primarios (1-Negro, 2-Blanco, 7-Azul) × 7 colores secundarios = 84 variantes

-- ============ TALLA A1 ============

-- A1 + Negro (1) primario
INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A1'); -- ID 1
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (1, 1, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (1, 1, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A1'); -- ID 2
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (2, 1, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (2, 2, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A1'); -- ID 3
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (3, 1, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (3, 3, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A1'); -- ID 4
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (4, 1, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (4, 4, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A1'); -- ID 5
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (5, 1, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (5, 5, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A1'); -- ID 6
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (6, 1, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (6, 6, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A1'); -- ID 7
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (7, 1, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (7, 7, 'secondary');

-- A1 + Blanco (2) primario
INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A1'); -- ID 8
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (8, 2, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (8, 1, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A1'); -- ID 9
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (9, 2, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (9, 2, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A1'); -- ID 10
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (10, 2, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (10, 3, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A1'); -- ID 11
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (11, 2, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (11, 4, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A1'); -- ID 12
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (12, 2, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (12, 5, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A1'); -- ID 13
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (13, 2, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (13, 6, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A1'); -- ID 14
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (14, 2, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (14, 7, 'secondary');

-- A1 + Azul (7) primario
INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A1'); -- ID 15
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (15, 7, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (15, 1, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A1'); -- ID 16
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (16, 7, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (16, 2, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A1'); -- ID 17
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (17, 7, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (17, 3, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A1'); -- ID 18
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (18, 7, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (18, 4, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A1'); -- ID 19
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (19, 7, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (19, 5, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A1'); -- ID 20
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (20, 7, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (20, 6, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A1'); -- ID 21
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (21, 7, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (21, 7, 'secondary');

-- ============ TALLA A2 ============

-- A2 + Negro (1) primario
INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A2'); -- ID 22
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (22, 1, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (22, 1, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A2'); -- ID 23
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (23, 1, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (23, 2, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A2'); -- ID 24
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (24, 1, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (24, 3, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A2'); -- ID 25
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (25, 1, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (25, 4, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A2'); -- ID 26
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (26, 1, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (26, 5, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A2'); -- ID 27
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (27, 1, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (27, 6, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A2'); -- ID 28
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (28, 1, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (28, 7, 'secondary');

-- A2 + Blanco (2) primario
INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A2'); -- ID 29
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (29, 2, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (29, 1, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A2'); -- ID 30
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (30, 2, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (30, 2, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A2'); -- ID 31
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (31, 2, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (31, 3, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A2'); -- ID 32
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (32, 2, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (32, 4, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A2'); -- ID 33
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (33, 2, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (33, 5, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A2'); -- ID 34
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (34, 2, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (34, 6, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A2'); -- ID 35
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (35, 2, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (35, 7, 'secondary');

-- A2 + Azul (7) primario
INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A2'); -- ID 36
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (36, 7, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (36, 1, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A2'); -- ID 37
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (37, 7, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (37, 2, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A2'); -- ID 38
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (38, 7, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (38, 3, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A2'); -- ID 39
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (39, 7, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (39, 4, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A2'); -- ID 40
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (40, 7, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (40, 5, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A2'); -- ID 41
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (41, 7, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (41, 6, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A2'); -- ID 42
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (42, 7, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (42, 7, 'secondary');

-- ============ TALLA A3 ============

-- A3 + Negro (1) primario
INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A3'); -- ID 43
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (43, 1, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (43, 1, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A3'); -- ID 44
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (44, 1, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (44, 2, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A3'); -- ID 45
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (45, 1, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (45, 3, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A3'); -- ID 46
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (46, 1, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (46, 4, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A3'); -- ID 47
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (47, 1, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (47, 5, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A3'); -- ID 48
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (48, 1, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (48, 6, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A3'); -- ID 49
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (49, 1, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (49, 7, 'secondary');

-- A3 + Blanco (2) primario
INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A3'); -- ID 50
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (50, 2, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (50, 1, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A3'); -- ID 51
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (51, 2, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (51, 2, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A3'); -- ID 52
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (52, 2, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (52, 3, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A3'); -- ID 53
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (53, 2, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (53, 4, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A3'); -- ID 54
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (54, 2, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (54, 5, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A3'); -- ID 55
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (55, 2, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (55, 6, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A3'); -- ID 56
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (56, 2, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (56, 7, 'secondary');

-- A3 + Azul (7) primario
INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A3'); -- ID 57
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (57, 7, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (57, 1, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A3'); -- ID 58
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (58, 7, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (58, 2, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A3'); -- ID 59
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (59, 7, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (59, 3, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A3'); -- ID 60
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (60, 7, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (60, 4, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A3'); -- ID 61
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (61, 7, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (61, 5, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A3'); -- ID 62
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (62, 7, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (62, 6, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A3'); -- ID 63
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (63, 7, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (63, 7, 'secondary');

-- ============ TALLA A4 ============

-- A4 + Negro (1) primario
INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A4'); -- ID 64
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (64, 1, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (64, 1, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A4'); -- ID 65
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (65, 1, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (65, 2, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A4'); -- ID 66
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (66, 1, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (66, 3, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A4'); -- ID 67
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (67, 1, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (67, 4, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A4'); -- ID 68
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (68, 1, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (68, 5, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A4'); -- ID 69
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (69, 1, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (69, 6, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A4'); -- ID 70
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (70, 1, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (70, 7, 'secondary');

-- A4 + Blanco (2) primario
INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A4'); -- ID 71
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (71, 2, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (71, 1, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A4'); -- ID 72
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (72, 2, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (72, 2, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A4'); -- ID 73
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (73, 2, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (73, 3, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A4'); -- ID 74
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (74, 2, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (74, 4, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A4'); -- ID 75
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (75, 2, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (75, 5, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A4'); -- ID 76
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (76, 2, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (76, 6, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A4'); -- ID 77
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (77, 2, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (77, 7, 'secondary');

-- A4 + Azul (7) primario
INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A4'); -- ID 78
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (78, 7, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (78, 1, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A4'); -- ID 79
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (79, 7, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (79, 2, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A4'); -- ID 80
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (80, 7, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (80, 3, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A4'); -- ID 81
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (81, 7, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (81, 4, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A4'); -- ID 82
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (82, 7, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (82, 5, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A4'); -- ID 83
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (83, 7, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (83, 6, 'secondary');

INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A4'); -- ID 84
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (84, 7, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (84, 7, 'secondary');
