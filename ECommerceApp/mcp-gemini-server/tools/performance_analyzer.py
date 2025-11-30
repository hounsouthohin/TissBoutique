import aiohttp
import time
import json
import asyncio
import os  # ← AJOUTÉ ICI
from typing import Dict, Any

class PerformanceAnalyzerTool:
    def __init__(self):
        self.api_host = os.getenv("API_HOST", "http://localhost:5000")
    
    async def analyze(self, endpoint: str, method: str = "GET", payload: str = "{}") -> str:
        start_time = time.time()
        
        try:
            async with aiohttp.ClientSession() as session:
                url = f"{self.api_host}{endpoint}"
                kwargs = {"method": method, "url": url}
                
                if method.upper() in ["POST", "PUT", "PATCH"]:
                    kwargs["json"] = json.loads(payload) if payload != "{}" else {}
                
                async with session.request(**kwargs, timeout=30) as response:
                    response_time = time.time() - start_time
                    
                    # Simulation des métriques SQL et C#
                    sql_queries = self._simulate_sql_queries(endpoint)
                    hot_paths = self._simulate_hot_paths(endpoint)
                    
                    report = {
                        "endpoint": endpoint,
                        "method": method,
                        "status_code": response.status,
                        "total_time_ms": round(response_time * 1000, 2),
                        "sql_queries": sql_queries,
                        "hot_paths": hot_paths,
                        "memory_allocated_mb": round(response_time * 10, 2),
                        "success": True
                    }
                    
                    return json.dumps(report, indent=2)
                    
        except Exception as e:
            return json.dumps({
                "error": str(e),
                "endpoint": endpoint,
                "success": False
            }, indent=2)
    
    def _simulate_sql_queries(self, endpoint: str) -> list:
        return [
            {"query": "SELECT * FROM Products WHERE Id = @id", "duration_ms": 2.5},
            {"query": "SELECT c.* FROM Categories c JOIN Products p ON c.Id = p.CategoryId", "duration_ms": 15.3}
        ]
    
    def _simulate_hot_paths(self, endpoint: str) -> list:
        return [
            {"method": "ProductService.GetByIdAsync", "duration_ms": 12.8, "percentage": 45},
            {"method": "EfRepository.ToListAsync", "duration_ms": 8.2, "percentage": 29}
        ]