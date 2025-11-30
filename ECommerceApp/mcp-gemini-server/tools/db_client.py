import asyncpg
import json
import os  # ← AJOUTÉ
from typing import Optional

class DatabaseClientTool:
    def __init__(self):
        self.connection_string = os.getenv("DB_CONNECTION", "")
    
    async def execute_query(self, query: str, limit: int = 100) -> str:
        if not self.connection_string:
            return json.dumps({
                "error": "DB_CONNECTION environment variable not set",
                "success": False
            })
        
        # Sécurité : seulement SELECT autorisé
        query_upper = query.strip().upper()
        if not query_upper.startswith("SELECT"):
            return json.dumps({
                "error": "Only SELECT queries are allowed",
                "success": False
            })
        
        try:
            conn = await asyncpg.connect(self.connection_string)
            
            # Ajout du LIMIT si pas présent
            if "LIMIT" not in query_upper:
                query += f" LIMIT {limit}"
            
            rows = await conn.fetch(query)
            columns = [] if not rows else [col.name for col in rows[0].keys()]
            
            result = {
                "success": True,
                "row_count": len(rows),
                "columns": columns,
                "data": [dict(row) for row in rows]
            }
            
            await conn.close()
            return json.dumps(result, indent=2)
            
        except Exception as e:
            return json.dumps({
                "error": str(e),
                "success": False
            }, indent=2)