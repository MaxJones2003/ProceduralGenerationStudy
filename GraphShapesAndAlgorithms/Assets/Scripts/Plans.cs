/*

Current Process: 

    1. Create a voronoi diagram

    2. Convert data to more usable format (Centers from delaunay, Corners from voronoi, Edges from both)

    3. Assign map data
        a. Assign elevation
            i. use radial to determine what is ocean
            ii. travere graph, assign elevation based on distance from ocean
        b. Determine what is ocean, coast, and land
        c. do other stuff

    -- START HEIGHT MESH CREATION --

    4. Create KD-Tree with corners

    5. For each point in the height map, find a list of the closest corners by treversing the KD-tree

    6. Use Corners list to perform Bicubic Interpolation to find the height at that point

    7. Apply heightmap to terrain


    I NEED TO REFACTOR SO BAD IT HURTS


    -- THINGS I WANT --

    1. Add more control to how biomes are chosen
        I want to use temperature and moisture, rather than elevation, to determine biomes
        I want to create a perlin map that determines temperature, and a perlin map that determines moisture
        Then, the maps elevation will alter the temperature and moisture maps
        Then, the temperature and moisture maps will be used to determine biomes
        I also want to control the max and min temperature and moisture values,
        this would help for creating less extreme biomes and more uniform islands

    2. Add more control to how elevation is chosen
        Currently, elevation is chosen based on distance from ocean. This means there is always
        a mountain in the center of the map, it would be ideal for the possibility of a flat grotto in between mountains

    3. Ability for editor to create hot spots, that define temperature and/or moisture in a specific region

    4. Seperate map into many pieces, instead of creating one large map. With the gathered data,
        create smaller maps, that use the biome to further influence the heightmap.
        IE: A mountain biome would create more jagged terrain, but would have a base height defined by 
        the the voronoi map.
        I think this would also be a good start to making infinit terrain.
        
    -- THINGS I NEED --

    1. Textures for the biomes applied procedurally
    2. POI's applied procedurally
    3. Generate flora procedurally
    4. Generate fauna procedurally
*/