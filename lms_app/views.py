from django.shortcuts import render
from .models import Book
# Create your views here.
def index(request):
     allbooks=Book.objects.all()
     books={'allbooks':allbooks}
     return render(request,'pages/index.html',books)

def books(request):
     allbooks=Book.objects.all()
     books={'allbooks':allbooks}
     return render(request,'pages/books.html',books)

def update(request):
    return render(request,'pages/update.html')

def delete(request):
    return render(request,'pages/delete.html')