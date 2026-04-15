-- Insert ProductVariants for Kimono (12 variants total)
-- 3 primary colors × 4 sizes = 12 variants

-- Variant 1-4: Primary color Negro (1)
INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A1');  -- ID 1
INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A2');  -- ID 2
INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A3');  -- ID 3
INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A4');  -- ID 4

-- Variant 5-8: Primary color Blanco (2)
INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A1');  -- ID 5
INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A2');  -- ID 6
INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A3');  -- ID 7
INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A4');  -- ID 8

-- Variant 9-12: Primary color Azul (3)
INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A1');  -- ID 9
INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A2');  -- ID 10
INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A3');  -- ID 11
INSERT INTO ProductVariants (ProductId, Size) VALUES (1, 'A4');  -- ID 12

-- Insert colors for each variant

-- Variant 1: Negro + all colors as secondary
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (1, 1, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (1, 1, 'secondary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (1, 2, 'secondary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (1, 3, 'secondary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (1, 4, 'secondary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (1, 5, 'secondary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (1, 6, 'secondary');

-- Variant 2: Negro + all colors as secondary
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (2, 1, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (2, 1, 'secondary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (2, 2, 'secondary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (2, 3, 'secondary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (2, 4, 'secondary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (2, 5, 'secondary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (2, 6, 'secondary');

-- Variant 3: Negro + all colors as secondary
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (3, 1, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (3, 1, 'secondary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (3, 2, 'secondary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (3, 3, 'secondary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (3, 4, 'secondary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (3, 5, 'secondary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (3, 6, 'secondary');

-- Variant 4: Negro + all colors as secondary
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (4, 1, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (4, 1, 'secondary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (4, 2, 'secondary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (4, 3, 'secondary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (4, 4, 'secondary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (4, 5, 'secondary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (4, 6, 'secondary');

-- Variant 5: Blanco + all colors as secondary
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (5, 2, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (5, 1, 'secondary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (5, 2, 'secondary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (5, 3, 'secondary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (5, 4, 'secondary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (5, 5, 'secondary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (5, 6, 'secondary');

-- Variant 6: Blanco + all colors as secondary
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (6, 2, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (6, 1, 'secondary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (6, 2, 'secondary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (6, 3, 'secondary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (6, 4, 'secondary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (6, 5, 'secondary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (6, 6, 'secondary');

-- Variant 7: Blanco + all colors as secondary
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (7, 2, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (7, 1, 'secondary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (7, 2, 'secondary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (7, 3, 'secondary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (7, 4, 'secondary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (7, 5, 'secondary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (7, 6, 'secondary');

-- Variant 8: Blanco + all colors as secondary
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (8, 2, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (8, 1, 'secondary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (8, 2, 'secondary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (8, 3, 'secondary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (8, 4, 'secondary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (8, 5, 'secondary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (8, 6, 'secondary');

-- Variant 9: Azul + all colors as secondary
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (9, 3, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (9, 1, 'secondary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (9, 2, 'secondary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (9, 3, 'secondary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (9, 4, 'secondary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (9, 5, 'secondary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (9, 6, 'secondary');

-- Variant 10: Azul + all colors as secondary
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (10, 3, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (10, 1, 'secondary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (10, 2, 'secondary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (10, 3, 'secondary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (10, 4, 'secondary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (10, 5, 'secondary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (10, 6, 'secondary');

-- Variant 11: Azul + all colors as secondary
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (11, 3, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (11, 1, 'secondary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (11, 2, 'secondary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (11, 3, 'secondary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (11, 4, 'secondary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (11, 5, 'secondary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (11, 6, 'secondary');

-- Variant 12: Azul + all colors as secondary
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (12, 3, 'primary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (12, 1, 'secondary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (12, 2, 'secondary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (12, 3, 'secondary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (12, 4, 'secondary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (12, 5, 'secondary');
INSERT INTO ProductVariantColors (ProductVariantId, ColorId, Role) VALUES (12, 6, 'secondary');
