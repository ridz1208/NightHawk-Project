using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class TerrainGenerator : MonoBehaviour
{
    #region public global variables

    //the players that force terrain generation to occur
    public GameObject[] playerObjects;
    private Player[] players;

    //the mesh tile that corresponds to the basic building block of the terrain
    public GameObject terrainTilePrefab;

    //the size of a given tile
    public float tileSize;

    //the number of terrainTiles in a given row under a given player at once. There will be (nRowTiles + 1)^2 terrain tiles in total
    public int nRowTiles;

    public float detailScale;

    public float heightScale;



    #endregion
    private Dictionary<long, Tile> worldTiles;

    //assuming we have players in the array, then we generate terrain
    void Start()
    {
        //setup dictionnary (Hashmap) of Tiles and currently used tiles
        worldTiles = new Dictionary<long, Tile>(10000);
        players = new Player[playerObjects.Length];

        int i = 0;
        foreach (GameObject player in playerObjects)
        {
            Vector3 position = player.transform.position;
            //we round to the nearest integer coordinate
            Player p = new Player(Mathf.RoundToInt(position.x / tileSize), Mathf.RoundToInt(position.z / tileSize), player);
            players[i++] = p;
        }

        //generate terrain
        generate();     
    }




    public void generate()
    {
       
        
        //construct tiles for all players in the game
        foreach ( Player p in players)
        {
            //check if array is corrupted
            if (p.terrainTiles != null)
                foreach (Tile t in p.terrainTiles)
                    Destroy(t.gameObject);

            //initialize the tiles for the player
            p.terrainTiles = new Tile[nRowTiles, nRowTiles];

            //generate each tile individually according to its location in the world
            int numberTilesInFront = ((nRowTiles - 1) / 2);
            for (int x = 0; x < nRowTiles; x++)
                for (int z = 0; z < nRowTiles; z++)
                {
                    int xPosTile = p.positionTileX - numberTilesInFront + x;
                    int zPosTile = p.positionTileZ - numberTilesInFront + z;
                    long key = encodePair(xPosTile, zPosTile);

                    //check if we already created the tile
                    if (worldTiles.ContainsKey(key))
                    {
                        //get the tile and increase the number of pointeres pointing to it
                        p.terrainTiles[x, z] = worldTiles[key];
                        p.terrainTiles[x, z].referencesPointedTo++;
                    }
                    else
                        p.terrainTiles[x, z] = generateTile(xPosTile, zPosTile);         
                }
        }   
    }

    private static readonly float sideLengthMesh = 10f;
    private Tile generateTile(int xpos, int zpos)
    {
        /*trying stuff: maybe delete me*/
        /*float[,] seeds = new float[(int)(nRowTiles), (int)(nRowTiles)];
        for (int xx = 0; xx < nRowTiles; xx++)
            for (int zz = 0; zz < nRowTiles; zz++)
               // if (xx == 0 || zz == 0 || xx == nRowTiles - 1 || zz == nRowTiles - 1)
                 //   seeds[xx, zz] = 0;
                 if (xx % (nRowTiles / 2) == 0 || zz % ((nRowTiles) / 2) == 0)
                    seeds[xx, zz] = UnityEngine.Random.Range(-10, 10);
        DiamondSquare.generateHeightMap(seeds, (int)(nRowTiles), (int)(nRowTiles), (int)(nRowTiles / 2), 1f);
		*/
        //create new tile object from designated prefab. Make parent this class and change the local scale of the transform to match that of the mesh
        GameObject tile = (GameObject) Instantiate(terrainTilePrefab, new Vector3(xpos * tileSize, 0, zpos * tileSize), Quaternion.identity);
        tile.transform.localScale = new Vector3(tileSize * 0.1f, 1, tileSize * 0.1f);
        tile.transform.parent = transform;

        //We get our hands on the Mesh component and its vertices of the newly fabricated tile
        Mesh mesh = tile.GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;
        
        //Using Perlin Noise, we generate height values for each vertex in the mesh
        for (int v = 0; v < vertices.Length; v++)
        {
            // generate the height for current vertex
            Vector3 vertexPosition = tile.transform.position + vertices[v] * tileSize / sideLengthMesh;

            //may need to fix this, see above, trying things out
            //float height = DiamondSquare.noise((int) (vertexPosition.x * detailScale),(int) (vertexPosition.z * detailScale));         
            float height = PerlinNoise.noise(vertexPosition.x * detailScale , vertexPosition.z * detailScale);

            //scale the height accordingly
            vertices[v].y = height * heightScale;
        }

        //reassign the mesh's vertices and recalculate its bounds
        mesh.vertices = vertices;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        //create the abstract tile object
        Tile tileObject = new Tile(xpos, zpos, tile);
        tileObject.referencesPointedTo++; //update the reference counter

        //add tile to current world tiles (used in case 2 players have overlapping tiles
        worldTiles.Add(encodePair(xpos, zpos), tileObject);

        //return the newly created tile object
        return tileObject;
    }

    /*hash code to encore 2 integers in one using Cantor's pairing function and a bijection between
      the natural and integer numbers. Guarantees uniqueness of keys.*/
    private long encodePair(int a, int b)
    {
        int newA = a >= 0 ? a * 2 : -a * 2 - 1;
        int newB = b >= 0 ? b * 2 : -b * 2 - 1;
        int t = newA + newB;
        return (long) (t * (t + 1) / 2) + newB;
    }








    // Update is called once per frame
   void Update ()
    {
        /*basic algorithm: 
          foreach player
	        check if x changed
                update player tile
            check if z changed
                update player tile
           
          delete unused tiles from the game
        */

        //iterate over all players in game
        foreach ( Player p in players)
        {
            //calculate the player's current tile position
            int newPositionTileX = Mathf.RoundToInt(p.transform.position.x / tileSize);
            int newPositionTileZ = Mathf.RoundToInt(p.transform.position.z / tileSize);

            //check if we changed on x axis, if so we generate a new x row and delete the farthest one
            if (newPositionTileX != p.positionTileX)
            {
                updateTiles(p,  newPositionTileX - p.positionTileX, 0);
                p.positionTileX = newPositionTileX;
            }

            //check if we changed on the z axis, if so we generate a new z row and delete the farthest one
            if (newPositionTileZ != p.positionTileZ)
            {
                updateTiles(p, 0, newPositionTileZ - p.positionTileZ);
                p.positionTileZ = newPositionTileZ;
            }
        }

        //remove non-referenced tiles
        cleanUnusedTiles();
    }

    private void updateTiles(Player p, int xChange, int zChange)
    {
        Tile[] newTiles = new Tile[nRowTiles];
        Tile[,] newTerrainTiles = new Tile[nRowTiles, nRowTiles];

        //compute the number of tiles in front of the airplane
        int numberTilesInFront = ((nRowTiles - 1) / 2);

        //we changed tiles (tile underneath the player is a different object) in the x axis
        if (xChange != 0)
        {  
            //iterate over all tiles to be deleted. These are marked by the xChange short variable that indicates whether to delete
            //the row behind or in front of the player
            for (int i = 0; i < nRowTiles; i++)
            {
                //decrement the reference counter to the given tile. The tile's game object will be destroyed, if necessary, in the cleanUnusedTiles methods afterwards
                p.terrainTiles[numberTilesInFront - numberTilesInFront * xChange, i].referencesPointedTo--;

                //null the reference in the player's terrain array
                p.terrainTiles[numberTilesInFront - numberTilesInFront * xChange, i] = null;

                //construct the new tile or fetch it from the world and assign it to the newly constructed array of tiles
                long key = encodePair(p.positionTileX + numberTilesInFront * xChange + xChange, p.positionTileZ - numberTilesInFront + i);
                if(worldTiles.ContainsKey(key))
                {
                    newTiles[i] = worldTiles[key];
                    newTiles[i].referencesPointedTo++;
                }
                else
                    newTiles[i] = generateTile(p.positionTileX + numberTilesInFront * xChange + xChange, p.positionTileZ - numberTilesInFront + i);
            }
        }

        //we changed tiles (tile underneath the player is a different object) in the z axis
        if (zChange != 0)
        {
            //iterate over all tiles to be deleted. These are marked by the xChange short variable that indicates whether to delete
            //the row behind or in front of the player
            for (int i = 0; i < nRowTiles; i++)
            {
                //decrement the reference counter to the given tile. The tile's game object will be destroyed, if necessary, in the cleanUnusedTiles methods afterwards
                p.terrainTiles[i, numberTilesInFront - numberTilesInFront * zChange].referencesPointedTo--;

                //null the reference in the player's terrain array
                p.terrainTiles[i, numberTilesInFront - numberTilesInFront * zChange] = null;

                //construct the new tile or fetch it from the world and assign it to the newly constructed array of tiles
                long key = encodePair(p.positionTileX - numberTilesInFront + i, p.positionTileZ + numberTilesInFront * zChange + zChange);
                if(worldTiles.ContainsKey(key))
                {
                    newTiles[i] = worldTiles[key];
                    newTiles[i].referencesPointedTo++;
                }
                else
                    newTiles[i] = generateTile(p.positionTileX - numberTilesInFront + i, p.positionTileZ + numberTilesInFront * zChange + zChange);
            }
        }

        //copy the previous terrainTile array in new the newTerrainTile
        Array.Copy(p.terrainTiles, newTerrainTiles, nRowTiles * nRowTiles);

        //go through all currently existing terrain tiles and replace them in array at their new location. We basically offset the tiles
        for (int i = 0; i < nRowTiles; i++)
            for (int j = 0; j < nRowTiles; j++)
                if (p.terrainTiles[i, j] != null)
                    newTerrainTiles[-p.positionTileX - xChange + numberTilesInFront + p.terrainTiles[i, j].positionX, -p.positionTileZ - zChange + numberTilesInFront + p.terrainTiles[i, j].positionZ] = p.terrainTiles[i, j];

        //finally, we add the new tiles to the array
        for (int i = 0; i < newTiles.Length; i++)
            newTerrainTiles[-p.positionTileX - xChange + numberTilesInFront + newTiles[i].positionX, -p.positionTileZ - zChange + numberTilesInFront + newTiles[i].positionZ] = newTiles[i];

        //assign the new terrain to the player
        p.terrainTiles = newTerrainTiles;
    }

    private void cleanUnusedTiles()
    {
        //get all unreferenced tiles
        List<long> keysToDelete = new List<long>(10);
        foreach (KeyValuePair<long, Tile> kvp in worldTiles)
            if (kvp.Value.referencesPointedTo == 0) //not referenced anymore, we delete this
                keysToDelete.Add(kvp.Key);

        //iterate over all of them
        foreach (long key in keysToDelete)
        {
            //remove and delete tile from dictionary
            Tile tileToDelete = worldTiles[key];
            worldTiles.Remove(key);

            //destroy game object associated with the Tile object
            Destroy(tileToDelete.gameObject);

            //set pointer to null for garbarge collection
            tileToDelete = null;
        }
    }

    void Awake()
    {
        //get object references. For simplicity, make all objects that force terrain to be generated to be tagged so. Upon awake, get the refenreces
        //of such objects here and place them in the appropriate array above
    }

    //singleton design pattern
    private TerrainGenerator() {}
}


class Player
{
    //the player's position on the terrain tile he is currently in (X,Z) coordinates
    public int positionTileX, positionTileZ;

    //the player's gameobject reference
    public Transform transform;

    //the tiles currently beneath the airplane
    public Tile[,] terrainTiles;

    public Player(int x, int z, GameObject player)
    {
        positionTileX = x;
        positionTileZ = z;
        transform = player.transform;
    }

}

class Tile
{
    //tile's position
    public int positionX, positionZ;

    //reference to the actual gameObject represented by Tile
    public GameObject gameObject;

    //counting references to this tile, needed to know whether or not we can destroy certain tiles or not
    public short referencesPointedTo = 0;

    public Tile(int x, int z, GameObject tile)
    {
        positionX = x;
        positionZ = z;
        gameObject = tile;  
    }
}